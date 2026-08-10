using CaixaMercado.Application;
using CaixaMercado.Application.Operacional.Contratos;
using CaixaMercado.Application.Operacional.Services;
using CaixaMercado.Domain.Model.Catalogo;
using CaixaMercado.Domain.Model.Vendas;
using CaixaMercado.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace CaixaMercado.Infrastructure.Tests;

public sealed class PostgreSqlFinalizacaoConcorrenteTests
{
    [PostgreSqlFact]
    public async Task DoisPdvs_DisputandoUltimoSku_SomenteUmFinalizaESeusEfeitosSaoAtomicos()
    {
        await PostgreSqlTestDatabase.ExecutarAsync(async connectionString =>
        {
            await using var provider = await CriarProviderAsync(connectionString);
            var produtoId = Guid.NewGuid();
            await PrepararProdutoComUltimaUnidadeAsync(provider, produtoId);

            var pdv1 = await PrepararVendaAsync(provider, produtoId, "789999000001", 1);
            var pdv2 = await PrepararVendaAsync(provider, produtoId, "789999000001", 2);

            await using var scope1 = provider.CreateAsyncScope();
            await using var scope2 = provider.CreateAsyncScope();
            var service1 = scope1.ServiceProvider.GetRequiredService<IFinalizacaoVendaApplicationService>();
            var service2 = scope2.ServiceProvider.GetRequiredService<IFinalizacaoVendaApplicationService>();

            var resultados = await Task.WhenAll(
                service1.FinalizarAsync(CriarFinalizacao(pdv1, "finalizar-pdv-1")),
                service2.FinalizarAsync(CriarFinalizacao(pdv2, "finalizar-pdv-2")));

            Assert.Single(resultados, resultado => resultado.Codigo == CodigoOperacao.Sucesso);
            Assert.Single(resultados, resultado => resultado.Codigo is
                CodigoOperacao.ConflitoVersao or CodigoOperacao.EstoqueInsuficiente);

            await using var verifyScope = provider.CreateAsyncScope();
            var context = verifyScope.ServiceProvider.GetRequiredService<MercadinhoDbContext>();
            Assert.Equal(1, await context.Vendas.CountAsync(venda =>
                venda.Status == StatusVendaOperacional.Finalizada));
            Assert.Equal(1, await context.PagamentosVenda.CountAsync());
            Assert.Equal(1, await context.MovimentosEstoque.CountAsync());
            Assert.Equal(1, await context.MovimentosCaixa.CountAsync());
            Assert.Equal(1, await context.EventosAuditoria.CountAsync(evento =>
                evento.Acao == "VendaFinalizada"));
            Assert.Equal(0m, await ConsultarDecimalAsync(connectionString,
                "SELECT quantidade FROM estoque_saldos WHERE produto_id = @produtoId", produtoId));
            Assert.Equal(1L, await ConsultarInt64Async(connectionString,
                "SELECT versao FROM estoque_saldos WHERE produto_id = @produtoId", produtoId));
            Assert.Equal(1L, await ConsultarInt64Async(connectionString,
                "SELECT COUNT(*) FROM requisicoes_idempotentes WHERE operacao = 'vendas.finalizar'"));
        });
    }

    [PostgreSqlFact]
    public async Task FinalizacaoEFechamentoConcorrentes_NuncaFechamCaixaComVendaNaoContabilizada()
    {
        await PostgreSqlTestDatabase.ExecutarAsync(async connectionString =>
        {
            await using var provider = await CriarProviderAsync(connectionString);
            var produtoId = Guid.NewGuid();
            await PrepararProdutoComUltimaUnidadeAsync(provider, produtoId);
            var venda = await PrepararVendaAsync(provider, produtoId, "789999000001", 1);

            await using var finalizacaoScope = provider.CreateAsyncScope();
            await using var fechamentoScope = provider.CreateAsyncScope();
            var finalizacaoService = finalizacaoScope.ServiceProvider
                .GetRequiredService<IFinalizacaoVendaApplicationService>();
            var sessaoService = fechamentoScope.ServiceProvider
                .GetRequiredService<ISessaoCaixaApplicationService>();

            var finalizacaoTask = finalizacaoService.FinalizarAsync(
                CriarFinalizacao(venda, "finalizar-concorrente-fechamento"));
            var fechamentoTask = sessaoService.FecharAsync(new FecharSessaoCaixaCommand(
                venda.SessaoId,
                venda.TerminalId,
                venda.OperadorId,
                110m,
                0,
                "fechar-concorrente-finalizacao"));

            var finalizacao = await finalizacaoTask;
            var fechamento = await fechamentoTask;
            Assert.NotEqual(finalizacao.Sucesso, fechamento.Sucesso);

            await using var verifyScope = provider.CreateAsyncScope();
            var context = verifyScope.ServiceProvider.GetRequiredService<MercadinhoDbContext>();
            var vendaPersistida = await context.Vendas.SingleAsync(item => item.Id == venda.VendaId);
            var sessaoPersistida = await context.SessoesCaixa.SingleAsync(item => item.Id == venda.SessaoId);
            var vendaFinalizada = vendaPersistida.Status == StatusVendaOperacional.Finalizada;

            if (sessaoPersistida.Status == CaixaMercado.Domain.Model.Caixas.StatusSessaoCaixa.Fechada)
            {
                Assert.False(vendaFinalizada);
                Assert.Equal(100m, sessaoPersistida.ValorEsperadoFechamento);
            }
            else
            {
                Assert.True(vendaFinalizada);
                Assert.Null(sessaoPersistida.ValorEsperadoFechamento);

                await using var retryScope = provider.CreateAsyncScope();
                var retry = await retryScope.ServiceProvider
                    .GetRequiredService<ISessaoCaixaApplicationService>()
                    .FecharAsync(new FecharSessaoCaixaCommand(
                        venda.SessaoId,
                        venda.TerminalId,
                        venda.OperadorId,
                        110m,
                        sessaoPersistida.Versao,
                        "fechar-apos-conflito-finalizacao"));

                Assert.True(retry.Sucesso, retry.Mensagem);
                Assert.Equal(CaixaMercado.Domain.Model.Caixas.StatusSessaoCaixa.Fechada,
                    retry.Dados!.Status);
                Assert.Equal(110m, retry.Dados.ValorEsperadoFechamento);
                Assert.Equal(110m, retry.Dados.ValorContadoFechamento);
                Assert.Equal(0m, retry.Dados.DiferencaFechamento);
            }

            Assert.Equal(vendaFinalizada ? 1 : 0,
                await context.MovimentosCaixa.CountAsync(movimento => movimento.VendaId == venda.VendaId));
            Assert.Equal(vendaFinalizada ? 0m : 1m, await ConsultarDecimalAsync(connectionString,
                "SELECT quantidade FROM estoque_saldos WHERE produto_id = @produtoId", produtoId));
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

    private static async Task PrepararProdutoComUltimaUnidadeAsync(ServiceProvider provider, Guid produtoId)
    {
        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<MercadinhoDbContext>();
        context.Produtos.Add(new Produto(produtoId, "ULTIMO-001", "789999000001", null,
            "ÚLTIMA UNIDADE", UnidadeMedida.Unidade, 10m, false));
        await context.SaveChangesAsync();
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO estoque_saldos (produto_id, quantidade, versao) VALUES ({produtoId}, {1m}, {0L})");
    }

    private static async Task<VendaPreparada> PrepararVendaAsync(
        ServiceProvider provider,
        Guid produtoId,
        string ean,
        int numeroPdv)
    {
        var filialId = Guid.NewGuid();
        var terminalId = Guid.NewGuid();
        var sessaoId = Guid.NewGuid();
        var operadorId = Guid.NewGuid();
        var vendaId = Guid.NewGuid();

        await using var scope = provider.CreateAsyncScope();
        var sessaoService = scope.ServiceProvider.GetRequiredService<ISessaoCaixaApplicationService>();
        var vendaService = scope.ServiceProvider.GetRequiredService<IVendaApplicationService>();

        var abertura = await sessaoService.AbrirAsync(new AbrirSessaoCaixaCommand(
            sessaoId, filialId, terminalId, operadorId, 100m, $"abrir-pdv-{numeroPdv}"));
        Assert.True(abertura.Sucesso, abertura.Mensagem);

        var criacao = await vendaService.CriarAsync(new CriarVendaCommand(
            vendaId, filialId, terminalId, sessaoId, operadorId, $"criar-pdv-{numeroPdv}"));
        Assert.True(criacao.Sucesso, criacao.Mensagem);

        var adicao = await vendaService.AdicionarItemAsync(new AdicionarItemVendaCommand(
            vendaId, terminalId, ean, 1m, criacao.Dados!.Versao, $"item-pdv-{numeroPdv}"));
        Assert.True(adicao.Sucesso, adicao.Mensagem);

        return new VendaPreparada(vendaId, terminalId, sessaoId, operadorId, adicao.Dados!.Versao, produtoId);
    }

    private static FinalizarVendaCommand CriarFinalizacao(VendaPreparada venda, string chave) => new(
        venda.VendaId,
        venda.TerminalId,
        venda.OperadorId,
        venda.Versao,
        [new PagamentoCommand(Guid.NewGuid(), FormaPagamentoOperacional.Dinheiro,
            10m, StatusPagamentoOperacional.Pendente, 10m)],
        chave,
        CorrelationId: $"teste-{chave}");

    private static async Task<decimal> ConsultarDecimalAsync(
        string connectionString,
        string sql,
        Guid produtoId)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("produtoId", produtoId);
        return Convert.ToDecimal(await command.ExecuteScalarAsync());
    }

    private static async Task<long> ConsultarInt64Async(
        string connectionString,
        string sql,
        Guid? produtoId = null)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        if (produtoId.HasValue) command.Parameters.AddWithValue("produtoId", produtoId.Value);
        return Convert.ToInt64(await command.ExecuteScalarAsync());
    }

    private sealed record VendaPreparada(
        Guid VendaId,
        Guid TerminalId,
        Guid SessaoId,
        Guid OperadorId,
        long Versao,
        Guid ProdutoId);
}
