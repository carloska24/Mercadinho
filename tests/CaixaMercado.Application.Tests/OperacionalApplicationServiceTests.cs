using CaixaMercado.Application.Operacional.Contratos;
using CaixaMercado.Application.Operacional.Portas;
using CaixaMercado.Application.Operacional.Services;
using CaixaMercado.Domain.Model.Catalogo;
using CaixaMercado.Domain.Model.Vendas;

namespace CaixaMercado.Application.Tests;

public sealed class OperacionalApplicationServiceTests
{
    private static readonly Guid FilialId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    private static readonly Guid TerminalId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    private static readonly Guid SessaoId = Guid.Parse("30000000-0000-0000-0000-000000000001");
    private static readonly Guid OperadorId = Guid.Parse("40000000-0000-0000-0000-000000000001");
    private static readonly DateTimeOffset Agora = new(2026, 8, 10, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task CriarVenda_ChaveRepetidaMesmoPayload_RetornaMesmoResultadoSemDuplicar()
    {
        var contexto = new ContextoTeste();
        var vendaId = Guid.NewGuid();
        var comando = new CriarVendaCommand(vendaId, FilialId, TerminalId, SessaoId, OperadorId, "criar-001");

        var primeira = await contexto.Service.CriarAsync(comando);
        var segunda = await contexto.Service.CriarAsync(comando);

        Assert.True(primeira.Sucesso);
        Assert.True(segunda.Sucesso);
        Assert.Equivalent(primeira.Dados, segunda.Dados, strict: true);
        Assert.Single(contexto.Vendas.Todas);
        Assert.Equal(1, contexto.UnitOfWork.Commits);
    }

    [Fact]
    public async Task AdicionarItem_ChaveRepetidaMesmoPayload_NaoDuplicaQuantidade()
    {
        var contexto = new ContextoTeste();
        var venda = contexto.AdicionarVendaAberta();
        var comando = new AdicionarItemVendaCommand(venda.Id, TerminalId, "7891234567890", 1m, 0, "item-001");

        var primeira = await contexto.Service.AdicionarItemAsync(comando);
        var segunda = await contexto.Service.AdicionarItemAsync(comando);

        Assert.True(primeira.Sucesso);
        Assert.True(segunda.Sucesso);
        Assert.Equivalent(primeira.Dados, segunda.Dados, strict: true);
        Assert.Single(venda.Itens);
        Assert.Equal(1m, venda.Itens[0].Quantidade);
        Assert.Equal(1, contexto.UnitOfWork.Commits);
    }

    [Fact]
    public async Task AdicionarItem_MesmaChaveComPayloadDiferente_RejeitaSemNovaMutacao()
    {
        var contexto = new ContextoTeste();
        var venda = contexto.AdicionarVendaAberta();
        var primeira = new AdicionarItemVendaCommand(venda.Id, TerminalId, "7891234567890", 1m, 0, "item-002");
        var conflitante = primeira with { Quantidade = 2m };

        var resultadoInicial = await contexto.Service.AdicionarItemAsync(primeira);
        var resultadoConflitante = await contexto.Service.AdicionarItemAsync(conflitante);

        Assert.True(resultadoInicial.Sucesso);
        Assert.Equal(CodigoOperacao.ChaveIdempotenciaReutilizada, resultadoConflitante.Codigo);
        Assert.Single(venda.Itens);
        Assert.Equal(1m, venda.Itens[0].Quantidade);
        Assert.Equal(1, contexto.UnitOfWork.Commits);
    }

    [Fact]
    public async Task AdicionarItem_VersaoEsperadaDesatualizada_RetornaConflito()
    {
        var contexto = new ContextoTeste();
        var venda = contexto.AdicionarVendaAberta();
        venda.AdicionarItem(contexto.Produtos.Produto, 1m);
        var comando = new AdicionarItemVendaCommand(venda.Id, TerminalId, "7891234567890", 1m, 0, "item-003");

        var resultado = await contexto.Service.AdicionarItemAsync(comando);

        Assert.Equal(CodigoOperacao.ConflitoVersao, resultado.Codigo);
        Assert.Single(venda.Itens);
        Assert.Equal(1m, venda.Itens[0].Quantidade);
        Assert.Equal(0, contexto.UnitOfWork.Commits);
    }

    [Fact]
    public async Task AdicionarItem_ConcorrenciaDetectadaNoCommit_RetornaConflito()
    {
        var contexto = new ContextoTeste();
        var venda = contexto.AdicionarVendaAberta();
        contexto.UnitOfWork.ExcecaoNoCommit = new ConflitoConcorrenciaException();
        var comando = new AdicionarItemVendaCommand(venda.Id, TerminalId, "7891234567890", 1m, 0, "item-004");

        var resultado = await contexto.Service.AdicionarItemAsync(comando);

        Assert.Equal(CodigoOperacao.ConflitoVersao, resultado.Codigo);
        Assert.Equal(1, contexto.UnitOfWork.TentativasCommit);
    }

    [Fact]
    public async Task PesquisarProdutos_LimiteInvalido_NaoConsultaRepositorio()
    {
        var produtos = new ProdutoRepositoryFake();
        var service = new CatalogoApplicationService(produtos);

        var resultado = await service.PesquisarAsync(new PesquisarProdutosQuery(null, 201));

        Assert.Equal(CodigoOperacao.RequisicaoInvalida, resultado.Codigo);
        Assert.Equal(0, produtos.Pesquisas);
    }

    private sealed class ContextoTeste
    {
        public ProdutoRepositoryFake Produtos { get; } = new();
        public VendaRepositoryFake Vendas { get; } = new();
        public IdempotencyStoreFake Idempotencia { get; } = new();
        public UnitOfWorkFake UnitOfWork { get; }
        public VendaApplicationService Service { get; }

        public ContextoTeste()
        {
            UnitOfWork = new UnitOfWorkFake(Idempotencia);
            Service = new VendaApplicationService(Produtos, Vendas, Idempotencia, UnitOfWork, new ClockFake());
        }

        public Venda AdicionarVendaAberta()
        {
            var venda = Venda.Abrir(Guid.NewGuid(), FilialId, TerminalId, SessaoId, OperadorId, Agora);
            Vendas.Todas.Add(venda.Id, venda);
            return venda;
        }
    }

    private sealed class ProdutoRepositoryFake : IProdutoRepository
    {
        public Produto Produto { get; } = new(Guid.Parse("50000000-0000-0000-0000-000000000001"),
            "001", "7891234567890", "101", "ARROZ 5KG", UnidadeMedida.Unidade, 24.90m, false);

        public int Pesquisas { get; private set; }

        public Task<IReadOnlyList<Produto>> PesquisarAsync(string? termo, int limite, CancellationToken cancellationToken)
        {
            Pesquisas++;
            IReadOnlyList<Produto> resultado = new[] { Produto };
            return Task.FromResult(resultado);
        }

        public Task<ResultadoBuscaProduto> ResolverPorIdentificadorAsync(string identificador, CancellationToken cancellationToken) =>
            Task.FromResult(identificador is "001" or "7891234567890" or "101"
                ? ResultadoBuscaProduto.Encontrado(Produto)
                : ResultadoBuscaProduto.NaoEncontrado());
    }

    private sealed class VendaRepositoryFake : IVendaRepository
    {
        public Dictionary<Guid, Venda> Todas { get; } = new();

        public Task<Venda?> ObterAsync(Guid vendaId, CancellationToken cancellationToken) =>
            Task.FromResult(Todas.GetValueOrDefault(vendaId));

        public Task AdicionarAsync(Venda venda, CancellationToken cancellationToken)
        {
            Todas.Add(venda.Id, venda);
            return Task.CompletedTask;
        }

        public Task AtualizarAsync(Venda venda, long versaoEsperada, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }

    private sealed class IdempotencyStoreFake : IIdempotencyStore
    {
        private readonly Dictionary<(Guid TerminalId, string Chave), RegistroIdempotencia> _registros = new();
        private readonly List<RegistroIdempotencia> _pendentes = new();

        public Task<RegistroIdempotencia?> ObterAsync(Guid terminalId, string chave,
            CancellationToken cancellationToken) =>
            Task.FromResult(_registros.GetValueOrDefault((terminalId, chave)));

        public Task AdicionarAsync(RegistroIdempotencia registro, CancellationToken cancellationToken)
        {
            _pendentes.Add(registro);
            return Task.CompletedTask;
        }

        public void Confirmar()
        {
            foreach (var registro in _pendentes)
                _registros.Add((registro.TerminalId, registro.Chave), registro);
            _pendentes.Clear();
        }

        public void Descartar() => _pendentes.Clear();
    }

    private sealed class UnitOfWorkFake(IdempotencyStoreFake idempotencia) : IUnitOfWork
    {
        public int TentativasCommit { get; private set; }
        public int Commits { get; private set; }
        public Exception? ExcecaoNoCommit { get; set; }

        public Task CommitAsync(CancellationToken cancellationToken)
        {
            TentativasCommit++;
            if (ExcecaoNoCommit is not null) throw ExcecaoNoCommit;
            idempotencia.Confirmar();
            Commits++;
            return Task.CompletedTask;
        }

        public Task DescartarAlteracoesAsync(CancellationToken cancellationToken)
        {
            idempotencia.Descartar();
            return Task.CompletedTask;
        }
    }

    private sealed class ClockFake : IClock
    {
        public DateTimeOffset UtcNow => Agora;
    }
}
