using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CaixaMercado.Api.Tests;

public sealed class ApiContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ApiContractTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Theory]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task HealthEndpoints_QuandoAplicacaoEstaSaudavel_RetornamContratoJson(string path)
    {
        using var response = await _client.GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        using var body = await response.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.NotNull(body);
        Assert.Equal("Healthy", body.RootElement.GetProperty("status").GetString());
        Assert.True(body.RootElement.TryGetProperty("totalDurationMs", out _));
        Assert.Equal(JsonValueKind.Array, body.RootElement.GetProperty("checks").ValueKind);
    }

    [Fact]
    public async Task Requisicao_ComCorrelationIdValido_DevePropagarCabecalho()
    {
        const string correlationId = "pdv01-venda-42";
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        request.Headers.Add("X-Correlation-ID", correlationId);

        using var response = await _client.SendAsync(request);

        Assert.Equal(correlationId, response.Headers.GetValues("X-Correlation-ID").Single());
    }

    [Fact]
    public async Task Requisicao_ComCorrelationIdInseguro_DeveGerarNovoIdentificador()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        request.Headers.Add("X-Correlation-ID", "valor com espacos");

        using var response = await _client.SendAsync(request);

        var returnedId = response.Headers.GetValues("X-Correlation-ID").Single();
        Assert.Matches("^[a-f0-9]{32}$", returnedId);
    }

    [Fact]
    public async Task WeatherForecast_NaoDeveMaisEstarExposto_EErroDeveUsarProblemDetails()
    {
        using var response = await _client.GetAsync("/weatherforecast");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using var body = await response.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.NotNull(body);
        Assert.Equal(404, body.RootElement.GetProperty("status").GetInt32());
        Assert.True(body.RootElement.TryGetProperty("correlationId", out _));
        Assert.True(body.RootElement.TryGetProperty("traceId", out _));
    }

    [Fact]
    public async Task BancoIndisponivel_DeveAfetarReadySemAfetarLive()
    {
        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddHealthChecks().AddCheck(
                        "postgresql",
                        () => HealthCheckResult.Unhealthy("Banco indisponivel"));
                });
            });
        using var client = factory.CreateClient();

        using var liveResponse = await client.GetAsync("/health/live");
        using var readyResponse = await client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, liveResponse.StatusCode);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, readyResponse.StatusCode);

        using var body = await readyResponse.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.NotNull(body);
        Assert.Equal("Unhealthy", body.RootElement.GetProperty("status").GetString());
        var databaseCheck = Assert.Single(body.RootElement.GetProperty("checks").EnumerateArray());
        Assert.Equal("postgresql", databaseCheck.GetProperty("name").GetString());
        Assert.Equal("Unhealthy", databaseCheck.GetProperty("status").GetString());
    }
}
