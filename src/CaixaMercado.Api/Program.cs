using CaixaMercado.Api.Infrastructure;
using CaixaMercado.Api.Features.Produtos;
using CaixaMercado.Api.Features.Vendas;
using CaixaMercado.Application;
using CaixaMercado.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        if (!context.ProblemDetails.Extensions.ContainsKey("codigo"))
        {
            context.ProblemDetails.Extensions["codigo"] =
                context.ProblemDetails.Status == StatusCodes.Status404NotFound
                    ? ErrorCodes.ResourceNotFound
                    : ErrorCodes.UnexpectedError;
        }

        if (context.ProblemDetails.Status == StatusCodes.Status404NotFound)
        {
            context.ProblemDetails.Type = "urn:caixa-mercado:erro:recurso-nao-encontrado";
        }

        context.ProblemDetails.Extensions["correlationId"] = context.HttpContext.TraceIdentifier;
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
    };
});
builder.Services.AddHealthChecks();
builder.Services.AddMercadinhoApplication();
builder.Services.AddMercadinhoPersistence(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = HealthCheckResponseWriter.WriteAsync
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    // A prontidão inclui o PostgreSQL registrado pela Infrastructure.
    Predicate = _ => true,
    ResponseWriter = HealthCheckResponseWriter.WriteAsync
});

app.MapProdutosEndpoints();
app.MapVendasEndpoints();

app.Run();

public partial class Program;
