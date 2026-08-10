using System.Security.Cryptography;
using System.Text.Json;
using CaixaMercado.Application.Operacional.Contratos;
using CaixaMercado.Application.Operacional.Portas;
using CaixaMercado.Domain.Model.Caixas;
using CaixaMercado.Domain.Model.Estoque;
using CaixaMercado.Domain.Model.Vendas;
using CaixaMercado.Domain.Model.Auditoria;

namespace CaixaMercado.Application.Operacional.Services;

public interface ISessaoCaixaApplicationService
{
    Task<ResultadoOperacao<SessaoCaixaDto>> AbrirAsync(AbrirSessaoCaixaCommand command,
        CancellationToken cancellationToken = default);
    Task<ResultadoOperacao<SessaoCaixaDto>> FecharAsync(FecharSessaoCaixaCommand command,
        CancellationToken cancellationToken = default);
}

public interface IFinalizacaoVendaApplicationService
{
    Task<ResultadoOperacao<FinalizacaoVendaDto>> FinalizarAsync(FinalizarVendaCommand command,
        CancellationToken cancellationToken = default);
}

internal sealed class IdempotencyCoordinator(
    IIdempotencyStore store,
    IUnitOfWork unitOfWork,
    IClock clock)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public string Hash<T>(T command) => Convert.ToHexString(SHA256.HashData(
        JsonSerializer.SerializeToUtf8Bytes(command, JsonOptions)));

    public async Task<ResultadoOperacao<T>?> ReplayAsync<T>(string operacao, Guid terminalId,
        string chave, string hash, CancellationToken cancellationToken)
    {
        var registro = await store.ObterAsync(terminalId, chave.Trim(), cancellationToken);
        if (registro is null) return null;
        if (registro.Operacao != operacao || registro.HashRequisicao != hash)
            return ResultadoOperacao<T>.Falha(CodigoOperacao.ChaveIdempotenciaReutilizada,
                "A chave de idempotência já foi usada com outro conteúdo.");
        var recurso = registro.RecursoJson is null ? default : JsonSerializer.Deserialize<T>(registro.RecursoJson, JsonOptions);
        return new ResultadoOperacao<T>(registro.CodigoResultado, recurso, registro.Mensagem);
    }

    public Task RegisterAsync<T>(string operacao, Guid terminalId, string chave, string hash,
        ResultadoOperacao<T> resultado, CancellationToken cancellationToken) =>
        store.AdicionarAsync(new RegistroIdempotencia(operacao, terminalId, chave.Trim(), hash,
            resultado.Codigo, resultado.Mensagem,
            resultado.Dados is null ? null : JsonSerializer.Serialize(resultado.Dados, JsonOptions),
            clock.UtcNow), cancellationToken);

    public async Task<ResultadoOperacao<T>> CommitAsync<T>(string operacao, Guid terminalId,
        string chave, string hash, ResultadoOperacao<T> resultado, CodigoOperacao codigoConflito,
        string mensagemConflito, CancellationToken cancellationToken)
    {
        try { await unitOfWork.CommitAsync(cancellationToken); return resultado; }
        catch (Exception exception) when (exception is ConflitoConcorrenciaException or ConflitoIdempotenciaException or ConflitoSessaoCaixaException)
        {
            await unitOfWork.DescartarAlteracoesAsync(cancellationToken);
            var replay = await ReplayAsync<T>(operacao, terminalId, chave, hash, cancellationToken);
            return replay ?? ResultadoOperacao<T>.Falha(
                exception is ConflitoSessaoCaixaException ? CodigoOperacao.SessaoCaixaJaAberta : codigoConflito,
                exception is ConflitoSessaoCaixaException ? "Já existe uma sessão aberta para o terminal." : mensagemConflito);
        }
    }
}

public sealed class SessaoCaixaApplicationService(
    ISessaoCaixaRepository sessoes,
    IMovimentoCaixaRepository movimentosCaixa,
    IAuditoriaRepository auditoria,
    IIdempotencyStore idempotencia,
    IUnitOfWork unitOfWork,
    IClock clock) : ISessaoCaixaApplicationService
{
    private readonly IdempotencyCoordinator _coordinator = new(idempotencia, unitOfWork, clock);

    public async Task<ResultadoOperacao<SessaoCaixaDto>> AbrirAsync(AbrirSessaoCaixaCommand command,
        CancellationToken cancellationToken = default)
    {
        const string operacao = "caixas.sessoes.abrir";
        if (!IdsValidos(command.SessaoCaixaId, command.FilialId, command.TerminalId, command.OperadorId) ||
            command.ValorAbertura < 0 || string.IsNullOrWhiteSpace(command.ChaveIdempotencia))
            return ResultadoOperacao<SessaoCaixaDto>.Falha(CodigoOperacao.RequisicaoInvalida, "Dados de abertura inválidos.");
        var hash = _coordinator.Hash(command);
        var replay = await _coordinator.ReplayAsync<SessaoCaixaDto>(operacao, command.TerminalId, command.ChaveIdempotencia, hash, cancellationToken);
        if (replay is not null) return replay;
        if (await sessoes.ObterAbertaPorTerminalAsync(command.TerminalId, cancellationToken) is not null)
            return ResultadoOperacao<SessaoCaixaDto>.Falha(CodigoOperacao.SessaoCaixaJaAberta, "Já existe uma sessão aberta para o terminal.");
        SessaoCaixa sessao;
        try { sessao = SessaoCaixa.Abrir(command.SessaoCaixaId, command.FilialId, command.TerminalId,
            command.OperadorId, command.ValorAbertura, clock.UtcNow); }
        catch (ArgumentException ex) { return ResultadoOperacao<SessaoCaixaDto>.Falha(CodigoOperacao.RequisicaoInvalida, ex.Message); }
        var resultado = ResultadoOperacao<SessaoCaixaDto>.Ok(Mapear(sessao));
        await sessoes.AdicionarAsync(sessao, cancellationToken);
        await auditoria.AdicionarAsync(new EventoAuditoria(Guid.NewGuid(), "SessaoCaixaAberta", sessao.Id,
            sessao.TerminalId, sessao.Id, command.OperadorId, clock.UtcNow), cancellationToken);
        await _coordinator.RegisterAsync(operacao, command.TerminalId, command.ChaveIdempotencia, hash, resultado, cancellationToken);
        return await _coordinator.CommitAsync(operacao, command.TerminalId, command.ChaveIdempotencia, hash,
            resultado, CodigoOperacao.ConflitoVersao, "A sessão foi alterada.", cancellationToken);
    }

    public async Task<ResultadoOperacao<SessaoCaixaDto>> FecharAsync(FecharSessaoCaixaCommand command,
        CancellationToken cancellationToken = default)
    {
        const string operacao = "caixas.sessoes.fechar";
        if (!IdsValidos(command.SessaoCaixaId, command.TerminalId, command.OperadorId) ||
            command.VersaoEsperada < 0 || string.IsNullOrWhiteSpace(command.ChaveIdempotencia))
            return ResultadoOperacao<SessaoCaixaDto>.Falha(CodigoOperacao.RequisicaoInvalida, "Dados de fechamento inválidos.");
        var hash = _coordinator.Hash(command);
        var replay = await _coordinator.ReplayAsync<SessaoCaixaDto>(operacao, command.TerminalId, command.ChaveIdempotencia, hash, cancellationToken);
        if (replay is not null) return replay;
        var sessao = await sessoes.ObterAsync(command.SessaoCaixaId, cancellationToken);
        if (sessao is null) return ResultadoOperacao<SessaoCaixaDto>.Falha(CodigoOperacao.SessaoCaixaNaoEncontrada, "Sessão de caixa não encontrada.");
        if (sessao.TerminalId != command.TerminalId) return ResultadoOperacao<SessaoCaixaDto>.Falha(CodigoOperacao.RegraNegocioViolada, "A sessão pertence a outro terminal.");
        if (sessao.Status != StatusSessaoCaixa.Aberta) return ResultadoOperacao<SessaoCaixaDto>.Falha(CodigoOperacao.SessaoCaixaFechada, "A sessão já está fechada.");
        if (sessao.Versao != command.VersaoEsperada) return ResultadoOperacao<SessaoCaixaDto>.Falha(CodigoOperacao.ConflitoVersao, "A sessão foi alterada.");
        var recebimentoLiquidoDinheiro = await movimentosCaixa.ObterRecebimentoLiquidoDinheiroAsync(
            sessao.Id, cancellationToken);
        var valorEsperado = sessao.ValorAbertura + recebimentoLiquidoDinheiro;
        try { sessao.Fechar(command.OperadorId, valorEsperado, command.ValorContado, clock.UtcNow); }
        catch (ArgumentException ex) { return ResultadoOperacao<SessaoCaixaDto>.Falha(CodigoOperacao.RequisicaoInvalida, ex.Message); }
        var resultado = ResultadoOperacao<SessaoCaixaDto>.Ok(Mapear(sessao));
        await sessoes.AtualizarAsync(sessao, command.VersaoEsperada, cancellationToken);
        await auditoria.AdicionarAsync(new EventoAuditoria(Guid.NewGuid(), "SessaoCaixaFechada", sessao.Id,
            sessao.TerminalId, sessao.Id, command.OperadorId, clock.UtcNow), cancellationToken);
        await _coordinator.RegisterAsync(operacao, command.TerminalId, command.ChaveIdempotencia, hash, resultado, cancellationToken);
        return await _coordinator.CommitAsync(operacao, command.TerminalId, command.ChaveIdempotencia, hash,
            resultado, CodigoOperacao.ConflitoVersao, "A sessão foi alterada.", cancellationToken);
    }

    private static bool IdsValidos(params Guid[] ids) => ids.All(id => id != Guid.Empty);
    private static SessaoCaixaDto Mapear(SessaoCaixa s) => new(s.Id, s.FilialId, s.TerminalId,
        s.OperadorAberturaId, s.ValorAbertura, s.AbertaEmUtc, s.Status, s.OperadorFechamentoId,
        s.ValorEsperadoFechamento, s.ValorContadoFechamento, s.DiferencaFechamento, s.FechadaEmUtc, s.Versao);
}

public sealed class FinalizacaoVendaApplicationService(
    IVendaRepository vendas,
    ISessaoCaixaRepository sessoes,
    IPagamentoVendaRepository pagamentosRepository,
    IEstoqueRepository estoque,
    IMovimentoCaixaRepository movimentosCaixaRepository,
    IAuditoriaRepository auditoria,
    IIdempotencyStore idempotencia,
    IUnitOfWork unitOfWork,
    IClock clock) : IFinalizacaoVendaApplicationService
{
    private readonly IdempotencyCoordinator _coordinator = new(idempotencia, unitOfWork, clock);

    public async Task<ResultadoOperacao<FinalizacaoVendaDto>> FinalizarAsync(FinalizarVendaCommand command,
        CancellationToken cancellationToken = default)
    {
        const string operacao = "vendas.finalizar";
        if (command.VendaId == Guid.Empty || command.TerminalId == Guid.Empty || command.OperadorId == Guid.Empty ||
            command.VersaoEsperada < 0 || command.Pagamentos is null || string.IsNullOrWhiteSpace(command.ChaveIdempotencia))
            return ResultadoOperacao<FinalizacaoVendaDto>.Falha(CodigoOperacao.RequisicaoInvalida, "Dados de finalização inválidos.");
        var hash = _coordinator.Hash(command with { CorrelationId = null });
        var replay = await _coordinator.ReplayAsync<FinalizacaoVendaDto>(operacao, command.TerminalId, command.ChaveIdempotencia, hash, cancellationToken);
        if (replay is not null) return replay;
        var venda = await vendas.ObterAsync(command.VendaId, cancellationToken);
        if (venda is null) return ResultadoOperacao<FinalizacaoVendaDto>.Falha(CodigoOperacao.VendaNaoEncontrada, "Venda não encontrada.");
        if (venda.TerminalId != command.TerminalId) return ResultadoOperacao<FinalizacaoVendaDto>.Falha(CodigoOperacao.RegraNegocioViolada, "A venda pertence a outro terminal.");
        if (venda.Versao != command.VersaoEsperada) return ResultadoOperacao<FinalizacaoVendaDto>.Falha(CodigoOperacao.ConflitoVersao, "A venda foi alterada.");
        var sessao = await sessoes.ObterAsync(venda.SessaoCaixaId, cancellationToken);
        if (sessao is null) return ResultadoOperacao<FinalizacaoVendaDto>.Falha(CodigoOperacao.SessaoCaixaNaoEncontrada, "Sessão de caixa não encontrada.");
        if (sessao.Status != StatusSessaoCaixa.Aberta) return ResultadoOperacao<FinalizacaoVendaDto>.Falha(CodigoOperacao.SessaoCaixaFechada, "A sessão de caixa está fechada.");

        List<PagamentoVenda> pagamentos;
        try
        {
            pagamentos = command.Pagamentos.Select(p => new PagamentoVenda(p.PagamentoId, venda.Id,
                venda.SessaoCaixaId, p.Forma, p.ValorAplicado,
                p.Forma == FormaPagamentoOperacional.Dinheiro ? StatusPagamentoOperacional.Aprovado : p.Status,
                clock.UtcNow, p.ValorRecebidoDinheiro, p.ReferenciaExterna)).ToList();
            // Valida a soma e o estado sem gravar efeitos externos.
            if (pagamentos.Any(p => p.Status != StatusPagamentoOperacional.Aprovado))
                throw new InvalidOperationException("Todos os pagamentos devem estar aprovados.");
            if (pagamentos.Sum(p => p.ValorAplicado) != venda.Total) throw new InvalidOperationException("A soma dos pagamentos deve ser igual ao total da venda.");
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        { return ResultadoOperacao<FinalizacaoVendaDto>.Falha(CodigoOperacao.RegraNegocioViolada, ex.Message); }

        var movimentos = venda.Itens.Select(item => MovimentoEstoque.SaidaPorVenda(Guid.NewGuid(),
            item.Produto.ProdutoId, venda.Id, item.Id, item.Quantidade, clock.UtcNow)).ToArray();
        foreach (var movimento in movimentos)
        {
            if (await estoque.TentarBaixarAsync(movimento, cancellationToken)) continue;
            await unitOfWork.DescartarAlteracoesAsync(cancellationToken);
            return ResultadoOperacao<FinalizacaoVendaDto>.Falha(CodigoOperacao.EstoqueInsuficiente,
                "Estoque insuficiente para finalizar a venda.");
        }

        try { venda.Finalizar(pagamentos); }
        catch (InvalidOperationException ex) { await unitOfWork.DescartarAlteracoesAsync(cancellationToken);
            return ResultadoOperacao<FinalizacaoVendaDto>.Falha(CodigoOperacao.RegraNegocioViolada, ex.Message); }
        await pagamentosRepository.AdicionarAsync(pagamentos, cancellationToken);
        var versaoOriginalSessao = sessao.Versao;
        sessao.RegistrarVenda();
        await sessoes.AtualizarAsync(sessao, versaoOriginalSessao, cancellationToken);
        var movimentosCaixa = pagamentos.Select(p => new MovimentoCaixa(Guid.NewGuid(), venda.SessaoCaixaId,
            venda.Id, p.Id, p.Forma, p.ValorAplicado,
            p.ValorRecebidoDinheiro ?? p.ValorAplicado, p.Troco, clock.UtcNow)).ToArray();
        await movimentosCaixaRepository.AdicionarAsync(movimentosCaixa, cancellationToken);
        await auditoria.AdicionarAsync(new EventoAuditoria(Guid.NewGuid(), "VendaFinalizada", venda.Id,
            command.TerminalId, venda.SessaoCaixaId, command.OperadorId, clock.UtcNow, command.CorrelationId), cancellationToken);
        await vendas.AtualizarAsync(venda, command.VersaoEsperada, cancellationToken);
        var resultado = ResultadoOperacao<FinalizacaoVendaDto>.Ok(new FinalizacaoVendaDto(venda.Id,
            venda.Numero, venda.Versao, venda.Status, venda.Total, pagamentos.Sum(p => p.Troco),
            pagamentos.Count, movimentos.Length));
        await _coordinator.RegisterAsync(operacao, command.TerminalId, command.ChaveIdempotencia, hash, resultado, cancellationToken);
        return await _coordinator.CommitAsync(operacao, command.TerminalId, command.ChaveIdempotencia, hash,
            resultado, CodigoOperacao.ConflitoVersao, "A venda foi alterada durante a finalização.", cancellationToken);
    }
}
