using CaixaMercado.Application;
using CaixaMercado.Application.Operacional.Contratos;
using CaixaMercado.Application.Operacional.Services;
using CaixaMercado.Domain.Model.Catalogo;
using CaixaMercado.Domain.Model.Vendas;
using CaixaMercado.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CaixaMercado.Infrastructure.Tests;

public sealed class PostgreSqlConstraintConflictTests
{
    [PostgreSqlFact]
    public async Task MesmaSessaoIdEmTerminaisDiferentes_DeveRetornarConflitoControlado()
    {
        await PostgreSqlTestDatabase.ExecutarAsync(async connectionString =>
        {
            await using var provider = await CriarProviderAsync(connectionString);
            await using var scope1 = provider.CreateAsyncScope();
            await using var scope2 = provider.CreateAsyncScope();
            var sessaoId = Guid.NewGuid();

            var resultados = await Task.WhenAll(
                scope1.ServiceProvider.GetRequiredService<ISessaoCaixaApplicationService>().AbrirAsync(
                    new AbrirSessaoCaixaCommand(sessaoId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                        100m, "sessao-pk-a")),
                scope2.ServiceProvider.GetRequiredService<ISessaoCaixaApplicationService>().AbrirAsync(
                    new AbrirSessaoCaixaCommand(sessaoId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                        100m, "sessao-pk-b")));

            Assert.Single(resultados, resultado => resultado.Codigo == CodigoOperacao.Sucesso);
            Assert.Single(resultados, resultado => resultado.Codigo == CodigoOperacao.SessaoCaixaJaAberta);
        });
    }

    [PostgreSqlFact]
    public async Task MesmoPagamentoIdEmVendasDiferentes_DeveRetornarConflitoControlado()
    {
        await PostgreSqlTestDatabase.ExecutarAsync(async connectionString =>
        {
            await using var provider = await CriarProviderAsync(connectionString);
            var venda1 = await PrepararVendaAsync(provider, "PROD-A", "7899900000001", 1);
            var venda2 = await PrepararVendaAsync(provider, "PROD-B", "7899900000002", 2);
            var pagamentoId = Guid.NewGuid();

            await using var scope1 = provider.CreateAsyncScope();
            await using var scope2 = provider.CreateAsyncScope();
            var resultados = await Task.WhenAll(
                scope1.ServiceProvider.GetRequiredService<IFinalizacaoVendaApplicationService>()
                    .FinalizarAsync(CriarFinalizacao(venda1, pagamentoId, "pagamento-pk-a")),
                scope2.ServiceProvider.GetRequiredService<IFinalizacaoVendaApplicationService>()
                    .FinalizarAsync(CriarFinalizacao(venda2, pagamentoId, "pagamento-pk-b")));

            Assert.Single(resultados, resultado => resultado.Codigo == CodigoOperacao.Sucesso);
            Assert.Single(resultados, resultado => resultado.Codigo == CodigoOperacao.ConflitoVersao);

            await using var verifyScope = provider.CreateAsyncScope();
            var context = verifyScope.ServiceProvider.GetRequiredService<MercadinhoDbContext>();
            Assert.Equal(1, await context.PagamentosVenda.CountAsync(pagamento => pagamento.Id == pagamentoId));
            Assert.Equal(1, await context.MovimentosCaixa.CountAsync(movimento => movimento.PagamentoId == pagamentoId));
        });
    }

    private static async Task<ServiceProvider> CriarProviderAsync(string connectionString)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Mercadinho"] = connectionString
            })
            .Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMercadinhoApplication();
        services.AddMercadinhoPersistence(configuration);
        var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });

        await using var scope = provider.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<MercadinhoDbContext>().Database.MigrateAsync();
        return provider;
    }

    private static async Task<VendaPreparada> PrepararVendaAsync(
        ServiceProvider provider,
        string codigo,
        string ean,
        int numero)
    {
        var produtoId = Guid.NewGuid();
        var filialId = Guid.NewGuid();
        var terminalId = Guid.NewGuid();
        var sessaoId = Guid.NewGuid();
        var operadorId = Guid.NewGuid();
        var vendaId = Guid.NewGuid();

        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<MercadinhoDbContext>();
        context.Produtos.Add(new Produto(produtoId, codigo, ean, null, codigo,
            UnidadeMedida.Unidade, 10m, false));
        await context.SaveChangesAsync();
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO estoque_saldos (produto_id, quantidade, versao) VALUES ({produtoId}, {1m}, {0L})");

        var abertura = await scope.ServiceProvider.GetRequiredService<ISessaoCaixaApplicationService>()
            .AbrirAsync(new AbrirSessaoCaixaCommand(sessaoId, filialId, terminalId, operadorId,
                100m, $"abrir-conflito-{numero}"));
        Assert.True(abertura.Sucesso, abertura.Mensagem);

        var vendas = scope.ServiceProvider.GetRequiredService<IVendaApplicationService>();
        var criacao = await vendas.CriarAsync(new CriarVendaCommand(vendaId, filialId, terminalId,
            sessaoId, operadorId, $"criar-conflito-{numero}"));
        Assert.True(criacao.Sucesso, criacao.Mensagem);
        var adicao = await vendas.AdicionarItemAsync(new AdicionarItemVendaCommand(vendaId, terminalId,
            ean, 1m, criacao.Dados!.Versao, $"item-conflito-{numero}"));
        Assert.True(adicao.Sucesso, adicao.Mensagem);

        return new VendaPreparada(vendaId, terminalId, operadorId, adicao.Dados!.Versao);
    }

    private static FinalizarVendaCommand CriarFinalizacao(
        VendaPreparada venda,
        Guid pagamentoId,
        string chave) => new(
        venda.VendaId,
        venda.TerminalId,
        venda.OperadorId,
        venda.Versao,
        [new PagamentoCommand(pagamentoId, FormaPagamentoOperacional.Dinheiro, 10m,
            StatusPagamentoOperacional.Pendente, 10m)],
        chave);

    private sealed record VendaPreparada(Guid VendaId, Guid TerminalId, Guid OperadorId, long Versao);
}
