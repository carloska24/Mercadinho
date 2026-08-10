using CaixaMercado.Application.Operacional.Contratos;
using CaixaMercado.Application.Operacional.Portas;
using CaixaMercado.Application.Operacional.Services;
using CaixaMercado.Domain.Model.Auditoria;
using CaixaMercado.Domain.Model.Caixas;
using CaixaMercado.Domain.Model.Catalogo;
using CaixaMercado.Domain.Model.Estoque;
using CaixaMercado.Domain.Model.Vendas;

namespace CaixaMercado.Application.Tests;

public sealed class FinalizacaoVendaApplicationServiceTests
{
    private static readonly DateTimeOffset Agora = new(2026, 8, 10, 15, 0, 0, TimeSpan.Zero);
    private static readonly Guid TerminalId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    private static readonly Guid OperadorId = Guid.Parse("40000000-0000-0000-0000-000000000001");

    [Fact]
    public async Task AbrirSessao_TerminalJaPossuiSessaoAberta_RejeitaSemCommit()
    {
        var existente = SessaoCaixa.Abrir(Guid.NewGuid(), Guid.NewGuid(), TerminalId, OperadorId, 100m, Agora);
        var repositorio = new SessaoRepositoryFake(existente);
        var idempotencia = new IdempotencyStoreFake();
        var uow = new UnitOfWorkFake(idempotencia);
        var caixa = new MovimentoCaixaRepositoryFake();
        var auditoria = new AuditoriaRepositoryFake();
        var service = new SessaoCaixaApplicationService(repositorio, caixa, auditoria, idempotencia, uow, new ClockFake());

        var resultado = await service.AbrirAsync(new AbrirSessaoCaixaCommand(Guid.NewGuid(),
            existente.FilialId, TerminalId, OperadorId, 50m, "abrir-duplicada"));

        Assert.Equal(CodigoOperacao.SessaoCaixaJaAberta, resultado.Codigo);
        Assert.Equal(0, uow.Commits);
    }

    [Fact]
    public async Task AbrirSessao_ReplayAgendaUmUnicoEventoAuditoria()
    {
        var repositorio = new SessaoRepositoryFake(null);
        var idempotencia = new IdempotencyStoreFake();
        var caixa = new MovimentoCaixaRepositoryFake();
        var auditoria = new AuditoriaRepositoryFake();
        var uow = new UnitOfWorkFake(idempotencia, auditoria);
        var service = new SessaoCaixaApplicationService(repositorio, caixa, auditoria, idempotencia, uow, new ClockFake());
        var command = new AbrirSessaoCaixaCommand(Guid.NewGuid(), Guid.NewGuid(), TerminalId,
            OperadorId, 100m, "abrir-auditada");

        var primeiro = await service.AbrirAsync(command);
        var replay = await service.AbrirAsync(command);

        Assert.True(primeiro.Sucesso); Assert.True(replay.Sucesso);
        Assert.Single(auditoria.Confirmados);
        Assert.Equal("SessaoCaixaAberta", auditoria.Confirmados[0].Acao);
        Assert.Equal(1, uow.Commits);
    }

    [Fact]
    public async Task FecharSessao_CalculaEsperadoNoServidorENaoPermiteMascararDivergencia()
    {
        var existente = SessaoCaixa.Abrir(Guid.NewGuid(), Guid.NewGuid(), TerminalId, OperadorId, 100m, Agora);
        var repositorio = new SessaoRepositoryFake(existente);
        var idempotencia = new IdempotencyStoreFake();
        var caixa = new MovimentoCaixaRepositoryFake { RecebimentoLiquidoDinheiro = 30m };
        var auditoria = new AuditoriaRepositoryFake();
        var uow = new UnitOfWorkFake(idempotencia, auditoria);
        var service = new SessaoCaixaApplicationService(repositorio, caixa, auditoria, idempotencia, uow, new ClockFake());

        var command = new FecharSessaoCaixaCommand(existente.Id,
            TerminalId, OperadorId, 120m, 0, "fechar-calculado");
        var resultado = await service.FecharAsync(command);
        var replay = await service.FecharAsync(command);

        Assert.True(resultado.Sucesso, resultado.Mensagem);
        Assert.True(replay.Sucesso, replay.Mensagem);
        Assert.Equal(130m, resultado.Dados!.ValorEsperadoFechamento);
        Assert.Equal(-10m, resultado.Dados.DiferencaFechamento);
        Assert.Equal(1, caixa.ConsultasRecebimento);
        Assert.Single(auditoria.Confirmados);
        Assert.Equal("SessaoCaixaFechada", auditoria.Confirmados[0].Acao);
        Assert.Equal(1, uow.Commits);
    }

    [Fact]
    public async Task Finalizar_DinheiroVinteParaTotalNoveENoventa_RegistraTudoETroco()
    {
        var c = new Contexto();
        var command = c.CommandDinheiro(20m, "fim-001");

        var resultado = await c.Service.FinalizarAsync(command);

        Assert.True(resultado.Sucesso, resultado.Mensagem);
        Assert.Equal(10.10m, resultado.Dados!.Troco);
        Assert.Equal(StatusVendaOperacional.Finalizada, c.Venda.Status);
        Assert.Single(c.Pagamentos.Confirmados);
        Assert.Single(c.Estoque.Confirmados);
        Assert.Single(c.Caixa.Confirmados);
        Assert.Single(c.Auditoria.Confirmados);
        Assert.Equal("VendaFinalizada", c.Auditoria.Confirmados[0].Acao);
        Assert.Equal(1, c.Uow.Commits);
        Assert.Equal(1, c.Sessoes.Atualizacoes);
    }

    [Fact]
    public async Task Finalizar_MesmaChaveMesmoPayload_DevolveReplaySemDuplicarEfeitos()
    {
        var c = new Contexto();
        var command = c.CommandDinheiro(20m, "fim-002");

        var primeiro = await c.Service.FinalizarAsync(command);
        var replay = await c.Service.FinalizarAsync(command with { CorrelationId = "corr-retry-diferente" });

        Assert.True(primeiro.Sucesso);
        Assert.True(replay.Sucesso);
        Assert.Equivalent(primeiro.Dados, replay.Dados, strict: true);
        Assert.Single(c.Pagamentos.Confirmados);
        Assert.Single(c.Caixa.Confirmados);
        Assert.Single(c.Auditoria.Confirmados);
        Assert.Equal(1, c.Uow.Commits);
    }

    [Fact]
    public async Task Finalizar_DinheiroInsuficiente_RejeitaSemMutacaoNemEfeitos()
    {
        var c = new Contexto();

        var resultado = await c.Service.FinalizarAsync(c.CommandDinheiro(9m, "fim-003"));

        Assert.Equal(CodigoOperacao.RegraNegocioViolada, resultado.Codigo);
        Assert.Equal(StatusVendaOperacional.Aberta, c.Venda.Status);
        c.AssertSemEfeitos();
    }

    [Fact]
    public async Task Finalizar_PixPendente_RejeitaSemEfeitos()
    {
        var c = new Contexto();
        var command = c.Command(new PagamentoCommand(Guid.NewGuid(), FormaPagamentoOperacional.Pix,
            9.90m, StatusPagamentoOperacional.Pendente, null, "pix-123"), "fim-004");

        var resultado = await c.Service.FinalizarAsync(command);

        Assert.Equal(CodigoOperacao.RegraNegocioViolada, resultado.Codigo);
        Assert.Equal(StatusVendaOperacional.Aberta, c.Venda.Status);
        c.AssertSemEfeitos();
    }

    [Fact]
    public async Task Finalizar_EstoqueInsuficiente_NaoAgendaPagamentoCaixaOuAuditoria()
    {
        var c = new Contexto { Estoque = { Suficiente = false } };

        var resultado = await c.Service.FinalizarAsync(c.CommandDinheiro(20m, "fim-005"));

        Assert.Equal(CodigoOperacao.EstoqueInsuficiente, resultado.Codigo);
        Assert.Equal(StatusVendaOperacional.Aberta, c.Venda.Status);
        c.AssertSemEfeitos();
        Assert.Equal(1, c.Uow.Descartes);
    }

    private sealed class Contexto
    {
        public Venda Venda { get; }
        public SessaoRepositoryFake Sessoes { get; }
        public PagamentoRepositoryFake Pagamentos { get; } = new();
        public EstoqueRepositoryFake Estoque { get; } = new();
        public MovimentoCaixaRepositoryFake Caixa { get; } = new();
        public AuditoriaRepositoryFake Auditoria { get; } = new();
        public IdempotencyStoreFake Idempotencia { get; } = new();
        public UnitOfWorkFake Uow { get; }
        public FinalizacaoVendaApplicationService Service { get; }

        public Contexto()
        {
            var sessao = SessaoCaixa.Abrir(Guid.NewGuid(), Guid.NewGuid(), TerminalId, OperadorId, 100m, Agora);
            Sessoes = new SessaoRepositoryFake(sessao);
            Venda = Venda.Abrir(Guid.NewGuid(), sessao.FilialId, TerminalId, sessao.Id, OperadorId, Agora);
            Venda.AdicionarItem(new Produto(Guid.NewGuid(), "010", "2000000000100", "002", "MAÇÃ FUJI",
                UnidadeMedida.Quilograma, 9.90m, true), 1m);
            Uow = new UnitOfWorkFake(Idempotencia, Pagamentos, Estoque, Caixa, Auditoria);
            Service = new FinalizacaoVendaApplicationService(new VendaRepositoryFake(Venda), Sessoes,
                Pagamentos, Estoque, Caixa, Auditoria, Idempotencia, Uow, new ClockFake());
        }

        public FinalizarVendaCommand CommandDinheiro(decimal recebido, string chave) => Command(
            new PagamentoCommand(Guid.NewGuid(), FormaPagamentoOperacional.Dinheiro, 9.90m,
                StatusPagamentoOperacional.Pendente, recebido), chave);

        public FinalizarVendaCommand Command(PagamentoCommand pagamento, string chave) =>
            new(Venda.Id, TerminalId, OperadorId, Venda.Versao, new[] { pagamento }, chave,
                CorrelationId: "corr-001");

        public void AssertSemEfeitos()
        {
            Assert.Empty(Pagamentos.Confirmados); Assert.Empty(Estoque.Confirmados);
            Assert.Empty(Caixa.Confirmados); Assert.Empty(Auditoria.Confirmados);
            Assert.Equal(0, Uow.Commits);
        }
    }

    private sealed class VendaRepositoryFake(Venda venda) : IVendaRepository
    {
        public Task<Venda?> ObterAsync(Guid id, CancellationToken ct) => Task.FromResult<Venda?>(id == venda.Id ? venda : null);
        public Task AdicionarAsync(Venda value, CancellationToken ct) => Task.CompletedTask;
        public Task AtualizarAsync(Venda value, long versao, CancellationToken ct) => Task.CompletedTask;
    }

    private sealed class SessaoRepositoryFake : ISessaoCaixaRepository
    {
        private SessaoCaixa? _sessao;
        public int Atualizacoes { get; private set; }
        public SessaoRepositoryFake(SessaoCaixa? sessao) => _sessao = sessao;
        public Task<SessaoCaixa?> ObterAsync(Guid id, CancellationToken ct) => Task.FromResult(_sessao?.Id == id ? _sessao : null);
        public Task<SessaoCaixa?> ObterAbertaPorTerminalAsync(Guid id, CancellationToken ct) => Task.FromResult(_sessao?.TerminalId == id && _sessao.Status == StatusSessaoCaixa.Aberta ? _sessao : null);
        public Task AdicionarAsync(SessaoCaixa value, CancellationToken ct) { _sessao = value; return Task.CompletedTask; }
        public Task AtualizarAsync(SessaoCaixa value, long versao, CancellationToken ct) { Atualizacoes++; return Task.CompletedTask; }
    }

    private interface ITransactionalFake { void Confirmar(); void Descartar(); }

    private sealed class PagamentoRepositoryFake : IPagamentoVendaRepository, ITransactionalFake
    {
        private readonly List<PagamentoVenda> _pendentes = new(); public List<PagamentoVenda> Confirmados { get; } = new();
        public Task AdicionarAsync(IReadOnlyCollection<PagamentoVenda> values, CancellationToken ct) { _pendentes.AddRange(values); return Task.CompletedTask; }
        public void Confirmar() { Confirmados.AddRange(_pendentes); _pendentes.Clear(); } public void Descartar() => _pendentes.Clear();
    }

    private sealed class EstoqueRepositoryFake : IEstoqueRepository, ITransactionalFake
    {
        private readonly List<MovimentoEstoque> _pendentes = new(); public List<MovimentoEstoque> Confirmados { get; } = new(); public bool Suficiente { get; set; } = true;
        public Task<bool> TentarBaixarAsync(MovimentoEstoque value, CancellationToken ct) { if (Suficiente) _pendentes.Add(value); return Task.FromResult(Suficiente); }
        public void Confirmar() { Confirmados.AddRange(_pendentes); _pendentes.Clear(); } public void Descartar() => _pendentes.Clear();
    }

    private sealed class MovimentoCaixaRepositoryFake : IMovimentoCaixaRepository, ITransactionalFake
    {
        private readonly List<MovimentoCaixa> _pendentes = new(); public List<MovimentoCaixa> Confirmados { get; } = new();
        public decimal RecebimentoLiquidoDinheiro { get; set; }
        public int ConsultasRecebimento { get; private set; }
        public Task AdicionarAsync(IReadOnlyCollection<MovimentoCaixa> values, CancellationToken ct) { _pendentes.AddRange(values); return Task.CompletedTask; }
        public Task<decimal> ObterRecebimentoLiquidoDinheiroAsync(Guid sessaoCaixaId, CancellationToken ct)
        { ConsultasRecebimento++; return Task.FromResult(RecebimentoLiquidoDinheiro); }
        public void Confirmar() { Confirmados.AddRange(_pendentes); _pendentes.Clear(); } public void Descartar() => _pendentes.Clear();
    }

    private sealed class AuditoriaRepositoryFake : IAuditoriaRepository, ITransactionalFake
    {
        private EventoAuditoria? _pendente; public List<EventoAuditoria> Confirmados { get; } = new();
        public Task AdicionarAsync(EventoAuditoria value, CancellationToken ct) { _pendente = value; return Task.CompletedTask; }
        public void Confirmar() { if (_pendente is not null) Confirmados.Add(_pendente); _pendente = null; } public void Descartar() => _pendente = null;
    }

    private sealed class IdempotencyStoreFake : IIdempotencyStore, ITransactionalFake
    {
        private RegistroIdempotencia? _pendente; private readonly Dictionary<(Guid, string), RegistroIdempotencia> _values = new();
        public Task<RegistroIdempotencia?> ObterAsync(Guid terminal, string chave, CancellationToken ct) => Task.FromResult(_values.GetValueOrDefault((terminal, chave)));
        public Task AdicionarAsync(RegistroIdempotencia value, CancellationToken ct) { _pendente = value; return Task.CompletedTask; }
        public void Confirmar() { if (_pendente is not null) _values.Add((_pendente.TerminalId, _pendente.Chave), _pendente); _pendente = null; } public void Descartar() => _pendente = null;
    }

    private sealed class UnitOfWorkFake(params ITransactionalFake[] stores) : IUnitOfWork
    {
        public int Commits { get; private set; } public int Descartes { get; private set; }
        public Task CommitAsync(CancellationToken ct) { foreach (var s in stores) s.Confirmar(); Commits++; return Task.CompletedTask; }
        public Task DescartarAlteracoesAsync(CancellationToken ct) { foreach (var s in stores) s.Descartar(); Descartes++; return Task.CompletedTask; }
    }

    private sealed class ClockFake : IClock { public DateTimeOffset UtcNow => Agora; }
}
