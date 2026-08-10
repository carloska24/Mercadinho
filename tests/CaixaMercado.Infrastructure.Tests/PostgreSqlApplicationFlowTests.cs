using CaixaMercado.Application;
using CaixaMercado.Application.Operacional.Contratos;
using CaixaMercado.Application.Operacional.Services;
using CaixaMercado.Domain.Model.Catalogo;
using CaixaMercado.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CaixaMercado.Infrastructure.Tests;

public sealed class PostgreSqlApplicationFlowTests
{
    private static readonly Guid FilialId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    private static readonly Guid TerminalId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    private static readonly Guid SessaoId = Guid.Parse("30000000-0000-0000-0000-000000000001");
    private static readonly Guid OperadorId = Guid.Parse("40000000-0000-0000-0000-000000000001");
    private static readonly Guid ProdutoId = Guid.Parse("50000000-0000-0000-0000-000000000001");

    [PostgreSqlFact]
    public async Task CriarVenda_ReplaySequencialPersistido_DevolveMesmoResultadoSemDuplicar()
    {
        await PostgreSqlTestDatabase.ExecutarAsync(async connectionString =>
        {
            await using var provider = await CriarProviderAsync(connectionString);
            var command = NovoComandoCriar(Guid.NewGuid(), "criar-persistido-001");

            ResultadoOperacao<VendaDto> primeiro;
            await using (var scope = provider.CreateAsyncScope())
                primeiro = await scope.ServiceProvider.GetRequiredService<IVendaApplicationService>().CriarAsync(command);

            ResultadoOperacao<VendaDto> replay;
            await using (var scope = provider.CreateAsyncScope())
                replay = await scope.ServiceProvider.GetRequiredService<IVendaApplicationService>().CriarAsync(command);

            Assert.True(primeiro.Sucesso);
            Assert.True(replay.Sucesso);
            Assert.Equivalent(primeiro.Dados, replay.Dados, strict: true);

            await using var verifyScope = provider.CreateAsyncScope();
            Assert.Equal(1, await verifyScope.ServiceProvider.GetRequiredService<MercadinhoDbContext>()
                .Vendas.CountAsync(venda => venda.Id == command.VendaId));
        });
    }

    [PostgreSqlFact]
    public async Task AdicionarItem_ReplaySequencialPersistido_NaoDuplicaItemNemQuantidade()
    {
        await PostgreSqlTestDatabase.ExecutarAsync(async connectionString =>
        {
            await using var provider = await CriarProviderAsync(connectionString);
            var vendaId = await CriarVendaAsync(provider, "criar-item-001");
            var command = NovoComandoAdicionar(vendaId, "item-persistido-001", 1m, 0);

            ResultadoOperacao<VendaDto> primeiro;
            await using (var scope = provider.CreateAsyncScope())
                primeiro = await scope.ServiceProvider.GetRequiredService<IVendaApplicationService>().AdicionarItemAsync(command);

            ResultadoOperacao<VendaDto> replay;
            await using (var scope = provider.CreateAsyncScope())
                replay = await scope.ServiceProvider.GetRequiredService<IVendaApplicationService>().AdicionarItemAsync(command);

            Assert.True(primeiro.Sucesso);
            Assert.True(replay.Sucesso);
            Assert.Equivalent(primeiro.Dados, replay.Dados, strict: true);

            await using var verifyScope = provider.CreateAsyncScope();
            var persistida = await verifyScope.ServiceProvider.GetRequiredService<MercadinhoDbContext>()
                .Vendas.Include(venda => venda.Itens).SingleAsync(venda => venda.Id == vendaId);
            Assert.Single(persistida.Itens);
            Assert.Equal(1m, persistida.Itens[0].Quantidade);
        });
    }

    [PostgreSqlFact]
    public async Task AdicionarItem_MesmaChaveComPayloadDiferente_RejeitaSegundoComando()
    {
        await PostgreSqlTestDatabase.ExecutarAsync(async connectionString =>
        {
            await using var provider = await CriarProviderAsync(connectionString);
            var vendaId = await CriarVendaAsync(provider, "criar-item-002");
            var primeiro = NovoComandoAdicionar(vendaId, "item-persistido-002", 1m, 0);
            var divergente = primeiro with { Quantidade = 2m };

            await using (var scope = provider.CreateAsyncScope())
                Assert.True((await scope.ServiceProvider.GetRequiredService<IVendaApplicationService>()
                    .AdicionarItemAsync(primeiro)).Sucesso);

            ResultadoOperacao<VendaDto> resultado;
            await using (var scope = provider.CreateAsyncScope())
                resultado = await scope.ServiceProvider.GetRequiredService<IVendaApplicationService>()
                    .AdicionarItemAsync(divergente);

            Assert.Equal(CodigoOperacao.ChaveIdempotenciaReutilizada, resultado.Codigo);
        });
    }

    [PostgreSqlFact]
    public async Task AdicionarItem_DuasChavesComMesmaVersao_ProduzUmSucessoEUmConflito()
    {
        await PostgreSqlTestDatabase.ExecutarAsync(async connectionString =>
        {
            await using var provider = await CriarProviderAsync(connectionString);
            var vendaId = await CriarVendaAsync(provider, "criar-concorrencia-001");
            await using var scope1 = provider.CreateAsyncScope();
            await using var scope2 = provider.CreateAsyncScope();
            var service1 = scope1.ServiceProvider.GetRequiredService<IVendaApplicationService>();
            var service2 = scope2.ServiceProvider.GetRequiredService<IVendaApplicationService>();

            Assert.True((await service1.ObterAsync(vendaId)).Sucesso);
            Assert.True((await service2.ObterAsync(vendaId)).Sucesso);

            var resultados = await Task.WhenAll(
                service1.AdicionarItemAsync(NovoComandoAdicionar(vendaId, "concorrente-a", 1m, 0)),
                service2.AdicionarItemAsync(NovoComandoAdicionar(vendaId, "concorrente-b", 1m, 0)));

            Assert.Single(resultados, resultado => resultado.Codigo == CodigoOperacao.Sucesso);
            Assert.Single(resultados, resultado => resultado.Codigo == CodigoOperacao.ConflitoVersao);
        });
    }

    [PostgreSqlFact]
    public async Task AdicionarItem_MesmaChaveSimultanea_DevolveResultadoVencedorParaAmbos()
    {
        await PostgreSqlTestDatabase.ExecutarAsync(async connectionString =>
        {
            await using var provider = await CriarProviderAsync(connectionString);
            var vendaId = await CriarVendaAsync(provider, "criar-concorrencia-002");
            await using var scope1 = provider.CreateAsyncScope();
            await using var scope2 = provider.CreateAsyncScope();
            var service1 = scope1.ServiceProvider.GetRequiredService<IVendaApplicationService>();
            var service2 = scope2.ServiceProvider.GetRequiredService<IVendaApplicationService>();

            Assert.True((await service1.ObterAsync(vendaId)).Sucesso);
            Assert.True((await service2.ObterAsync(vendaId)).Sucesso);
            var command = NovoComandoAdicionar(vendaId, "concorrente-mesma-chave", 1m, 0);

            var resultados = await Task.WhenAll(
                service1.AdicionarItemAsync(command),
                service2.AdicionarItemAsync(command));

            Assert.All(resultados, resultado => Assert.Equal(CodigoOperacao.Sucesso, resultado.Codigo));
            Assert.Equivalent(resultados[0].Dados, resultados[1].Dados, strict: true);

            await using var verifyScope = provider.CreateAsyncScope();
            var persistida = await verifyScope.ServiceProvider.GetRequiredService<MercadinhoDbContext>()
                .Vendas.Include(venda => venda.Itens).SingleAsync(venda => venda.Id == vendaId);
            Assert.Single(persistida.Itens);
            Assert.Equal(1m, persistida.Itens[0].Quantidade);
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
        var context = scope.ServiceProvider.GetRequiredService<MercadinhoDbContext>();
        await context.Database.MigrateAsync();
        context.Produtos.Add(new Produto(ProdutoId, "001", "7891234567890", "101",
            "ARROZ TIPO 1 5KG", UnidadeMedida.Unidade, 24.90m, false));
        await context.SaveChangesAsync();
        return provider;
    }

    private static async Task<Guid> CriarVendaAsync(ServiceProvider provider, string chave)
    {
        var vendaId = Guid.NewGuid();
        await using var scope = provider.CreateAsyncScope();
        var resultado = await scope.ServiceProvider.GetRequiredService<IVendaApplicationService>()
            .CriarAsync(NovoComandoCriar(vendaId, chave));
        Assert.True(resultado.Sucesso, resultado.Mensagem);
        return vendaId;
    }

    private static CriarVendaCommand NovoComandoCriar(Guid vendaId, string chave) =>
        new(vendaId, FilialId, TerminalId, SessaoId, OperadorId, chave);

    private static AdicionarItemVendaCommand NovoComandoAdicionar(
        Guid vendaId,
        string chave,
        decimal quantidade,
        long versaoEsperada) =>
        new(vendaId, TerminalId, "7891234567890", quantidade, versaoEsperada, chave);
}
