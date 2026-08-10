using System.Security.Cryptography;
using System.Text.Json;
using CaixaMercado.Application.Operacional.Contratos;
using CaixaMercado.Application.Operacional.Portas;
using CaixaMercado.Domain.Model.Catalogo;
using CaixaMercado.Domain.Model.Vendas;

namespace CaixaMercado.Application.Operacional.Services;

public interface ICatalogoApplicationService
{
    Task<ResultadoOperacao<IReadOnlyList<ProdutoDto>>> PesquisarAsync(
        PesquisarProdutosQuery query,
        CancellationToken cancellationToken = default);

    Task<ResultadoOperacao<ProdutoDto>> ResolverAsync(
        ResolverProdutoQuery query,
        CancellationToken cancellationToken = default);
}

public interface IVendaApplicationService
{
    Task<ResultadoOperacao<VendaDto>> CriarAsync(
        CriarVendaCommand command,
        CancellationToken cancellationToken = default);

    Task<ResultadoOperacao<VendaDto>> ObterAsync(
        Guid vendaId,
        CancellationToken cancellationToken = default);

    Task<ResultadoOperacao<VendaDto>> AdicionarItemAsync(
        AdicionarItemVendaCommand command,
        CancellationToken cancellationToken = default);
}

public sealed class CatalogoApplicationService(IProdutoRepository produtos) : ICatalogoApplicationService
{
    public async Task<ResultadoOperacao<IReadOnlyList<ProdutoDto>>> PesquisarAsync(
        PesquisarProdutosQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.Limite is < 1 or > 200)
            return ResultadoOperacao<IReadOnlyList<ProdutoDto>>.Falha(
                CodigoOperacao.RequisicaoInvalida,
                "O limite deve estar entre 1 e 200.");

        var encontrados = await produtos.PesquisarAsync(query.Termo?.Trim(), query.Limite, cancellationToken);
        return ResultadoOperacao<IReadOnlyList<ProdutoDto>>.Ok(encontrados.Select(Mapear).ToArray());
    }

    public async Task<ResultadoOperacao<ProdutoDto>> ResolverAsync(
        ResolverProdutoQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (string.IsNullOrWhiteSpace(query.Identificador))
            return ResultadoOperacao<ProdutoDto>.Falha(CodigoOperacao.RequisicaoInvalida, "O identificador do produto é obrigatório.");

        var resultado = await produtos.ResolverPorIdentificadorAsync(query.Identificador.Trim(), cancellationToken);
        return resultado.Situacao switch
        {
            SituacaoBuscaProduto.Encontrado => ResultadoOperacao<ProdutoDto>.Ok(Mapear(resultado.Produto!)),
            SituacaoBuscaProduto.IdentificadorAmbiguo => ResultadoOperacao<ProdutoDto>.Falha(
                CodigoOperacao.IdentificadorProdutoAmbiguo, "Mais de um produto corresponde ao identificador informado."),
            _ => ResultadoOperacao<ProdutoDto>.Falha(CodigoOperacao.ProdutoNaoEncontrado, "Produto não encontrado.")
        };
    }

    internal static ProdutoDto Mapear(Produto produto) => new(
        produto.Id,
        produto.CodigoInterno,
        produto.Ean,
        produto.Plu,
        produto.Descricao,
        produto.UnidadeMedida,
        produto.PrecoVenda,
        produto.ProdutoPesavel);
}

public sealed class VendaApplicationService(
    IProdutoRepository produtos,
    IVendaRepository vendas,
    IIdempotencyStore idempotencia,
    IUnitOfWork unitOfWork,
    IClock clock) : IVendaApplicationService
{
    private const string OperacaoCriarVenda = "vendas.criar";
    private const string OperacaoAdicionarItem = "vendas.itens.adicionar";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<ResultadoOperacao<VendaDto>> CriarAsync(
        CriarVendaCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var validacao = ValidarCriacao(command);
        if (validacao is not null) return validacao;

        var hash = CalcularHash(command);
        var repeticao = await ObterRepeticaoAsync(OperacaoCriarVenda, command.TerminalId,
            command.ChaveIdempotencia, hash, cancellationToken);
        if (repeticao is not null) return repeticao;

        if (await vendas.ObterAsync(command.VendaId, cancellationToken) is not null)
            return ResultadoOperacao<VendaDto>.Falha(CodigoOperacao.RegraNegocioViolada, "Já existe uma venda com o identificador informado.");

        var venda = Venda.Abrir(command.VendaId, command.FilialId, command.TerminalId,
            command.SessaoCaixaId, command.OperadorId, clock.UtcNow);
        var resultado = ResultadoOperacao<VendaDto>.Ok(Mapear(venda));

        await vendas.AdicionarAsync(venda, cancellationToken);
        await RegistrarIdempotenciaAsync(OperacaoCriarVenda, command.TerminalId,
            command.ChaveIdempotencia, hash, resultado, cancellationToken);

        return await CommitAsync(OperacaoCriarVenda, command.TerminalId, command.ChaveIdempotencia,
            hash, resultado, cancellationToken);
    }

    public async Task<ResultadoOperacao<VendaDto>> ObterAsync(
        Guid vendaId,
        CancellationToken cancellationToken = default)
    {
        if (vendaId == Guid.Empty)
            return ResultadoOperacao<VendaDto>.Falha(CodigoOperacao.RequisicaoInvalida, "O identificador da venda é obrigatório.");

        var venda = await vendas.ObterAsync(vendaId, cancellationToken);
        return venda is null
            ? ResultadoOperacao<VendaDto>.Falha(CodigoOperacao.VendaNaoEncontrada, "Venda não encontrada.")
            : ResultadoOperacao<VendaDto>.Ok(Mapear(venda));
    }

    public async Task<ResultadoOperacao<VendaDto>> AdicionarItemAsync(
        AdicionarItemVendaCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var validacao = ValidarAdicao(command);
        if (validacao is not null) return validacao;

        var hash = CalcularHash(command);
        var repeticao = await ObterRepeticaoAsync(OperacaoAdicionarItem, command.TerminalId,
            command.ChaveIdempotencia, hash, cancellationToken);
        if (repeticao is not null) return repeticao;

        var venda = await vendas.ObterAsync(command.VendaId, cancellationToken);
        if (venda is null)
            return ResultadoOperacao<VendaDto>.Falha(CodigoOperacao.VendaNaoEncontrada, "Venda não encontrada.");
        if (venda.TerminalId != command.TerminalId)
            return ResultadoOperacao<VendaDto>.Falha(CodigoOperacao.RegraNegocioViolada, "A venda pertence a outro terminal.");
        if (venda.Versao != command.VersaoEsperada)
            return ResultadoOperacao<VendaDto>.Falha(CodigoOperacao.ConflitoVersao, "A venda foi alterada. Atualize o carrinho e tente novamente.");

        var busca = await produtos.ResolverPorIdentificadorAsync(command.IdentificadorProduto.Trim(), cancellationToken);
        if (busca.Situacao == SituacaoBuscaProduto.NaoEncontrado)
            return ResultadoOperacao<VendaDto>.Falha(CodigoOperacao.ProdutoNaoEncontrado, "Produto não encontrado.");
        if (busca.Situacao == SituacaoBuscaProduto.IdentificadorAmbiguo)
            return ResultadoOperacao<VendaDto>.Falha(CodigoOperacao.IdentificadorProdutoAmbiguo,
                "Mais de um produto corresponde ao identificador informado.");

        try
        {
            venda.AdicionarItem(busca.Produto!, command.Quantidade);
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            return ResultadoOperacao<VendaDto>.Falha(CodigoOperacao.RegraNegocioViolada, exception.Message);
        }

        var resultado = ResultadoOperacao<VendaDto>.Ok(Mapear(venda));
        await vendas.AtualizarAsync(venda, command.VersaoEsperada, cancellationToken);
        await RegistrarIdempotenciaAsync(OperacaoAdicionarItem, command.TerminalId,
            command.ChaveIdempotencia, hash, resultado, cancellationToken);

        return await CommitAsync(OperacaoAdicionarItem, command.TerminalId, command.ChaveIdempotencia,
            hash, resultado, cancellationToken);
    }

    private async Task<ResultadoOperacao<VendaDto>?> ObterRepeticaoAsync(
        string operacao,
        Guid terminalId,
        string chave,
        string hash,
        CancellationToken cancellationToken)
    {
        var registro = await idempotencia.ObterAsync(terminalId, chave.Trim(), cancellationToken);
        if (registro is null) return null;
        if (!string.Equals(registro.Operacao, operacao, StringComparison.Ordinal) ||
            !string.Equals(registro.HashRequisicao, hash, StringComparison.Ordinal))
            return ResultadoOperacao<VendaDto>.Falha(CodigoOperacao.ChaveIdempotenciaReutilizada,
                "A chave de idempotência já foi usada com outro conteúdo.");

        var dados = registro.RecursoJson is null
            ? null
            : JsonSerializer.Deserialize<VendaDto>(registro.RecursoJson, JsonOptions);
        return new ResultadoOperacao<VendaDto>(registro.CodigoResultado, dados, registro.Mensagem);
    }

    private Task RegistrarIdempotenciaAsync(
        string operacao,
        Guid terminalId,
        string chave,
        string hash,
        ResultadoOperacao<VendaDto> resultado,
        CancellationToken cancellationToken) =>
        idempotencia.AdicionarAsync(new RegistroIdempotencia(
            operacao,
            terminalId,
            chave.Trim(),
            hash,
            resultado.Codigo,
            resultado.Mensagem,
            resultado.Dados is null ? null : JsonSerializer.Serialize(resultado.Dados, JsonOptions),
            clock.UtcNow), cancellationToken);

    private async Task<ResultadoOperacao<VendaDto>> CommitAsync(
        string operacao,
        Guid terminalId,
        string chave,
        string hash,
        ResultadoOperacao<VendaDto> resultado,
        CancellationToken cancellationToken)
    {
        try
        {
            await unitOfWork.CommitAsync(cancellationToken);
            return resultado;
        }
        catch (ConflitoConcorrenciaException)
        {
            await unitOfWork.DescartarAlteracoesAsync(cancellationToken);
            var repeticao = await ObterRepeticaoAsync(operacao, terminalId, chave.Trim(), hash, cancellationToken);
            return repeticao ?? ResultadoOperacao<VendaDto>.Falha(CodigoOperacao.ConflitoVersao,
                "A venda foi alterada. Atualize o carrinho e tente novamente.");
        }
        catch (ConflitoIdempotenciaException)
        {
            await unitOfWork.DescartarAlteracoesAsync(cancellationToken);
            var repeticao = await ObterRepeticaoAsync(operacao, terminalId, chave.Trim(), hash, cancellationToken);
            return repeticao ?? ResultadoOperacao<VendaDto>.Falha(CodigoOperacao.ConflitoIdempotencia,
                "Não foi possível confirmar o resultado idempotente. Consulte a venda antes de tentar novamente.");
        }
    }

    private static ResultadoOperacao<VendaDto>? ValidarCriacao(CriarVendaCommand command)
    {
        if (command.VendaId == Guid.Empty || command.FilialId == Guid.Empty || command.TerminalId == Guid.Empty ||
            command.SessaoCaixaId == Guid.Empty || command.OperadorId == Guid.Empty)
            return ResultadoOperacao<VendaDto>.Falha(CodigoOperacao.RequisicaoInvalida, "Todos os identificadores são obrigatórios.");
        return ValidarChave(command.ChaveIdempotencia);
    }

    private static ResultadoOperacao<VendaDto>? ValidarAdicao(AdicionarItemVendaCommand command)
    {
        if (command.VendaId == Guid.Empty || command.TerminalId == Guid.Empty)
            return ResultadoOperacao<VendaDto>.Falha(CodigoOperacao.RequisicaoInvalida, "Venda e terminal são obrigatórios.");
        if (string.IsNullOrWhiteSpace(command.IdentificadorProduto))
            return ResultadoOperacao<VendaDto>.Falha(CodigoOperacao.RequisicaoInvalida, "O identificador do produto é obrigatório.");
        if (command.Quantidade <= 0m)
            return ResultadoOperacao<VendaDto>.Falha(CodigoOperacao.RequisicaoInvalida, "A quantidade deve ser maior que zero.");
        if (command.VersaoEsperada < 0)
            return ResultadoOperacao<VendaDto>.Falha(CodigoOperacao.RequisicaoInvalida, "A versão esperada é inválida.");
        return ValidarChave(command.ChaveIdempotencia);
    }

    private static ResultadoOperacao<VendaDto>? ValidarChave(string chave)
    {
        if (string.IsNullOrWhiteSpace(chave) || chave.Trim().Length > 100)
            return ResultadoOperacao<VendaDto>.Falha(CodigoOperacao.RequisicaoInvalida,
                "A chave de idempotência é obrigatória e deve ter até 100 caracteres.");
        return null;
    }

    private static string CalcularHash<T>(T command)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(command, JsonOptions);
        return Convert.ToHexString(SHA256.HashData(bytes));
    }

    private static VendaDto Mapear(Venda venda) => new(
        venda.Id,
        venda.Numero,
        venda.FilialId,
        venda.TerminalId,
        venda.SessaoCaixaId,
        venda.OperadorId,
        venda.CriadaEmUtc,
        venda.Status,
        venda.Versao,
        venda.QuantidadeTotal,
        venda.Subtotal,
        venda.Desconto,
        venda.Total,
        venda.Itens.Select(item => new ItemVendaDto(
            item.Id,
            item.Sequencial,
            item.Produto.ProdutoId,
            item.Produto.CodigoInterno,
            item.Produto.Descricao,
            item.Produto.UnidadeMedida,
            item.Produto.PrecoUnitario,
            item.Quantidade,
            item.Desconto,
            item.Total)).ToArray());
}
