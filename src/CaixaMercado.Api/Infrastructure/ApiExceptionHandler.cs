using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CaixaMercado.Api.Infrastructure;

internal sealed class ApiExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(
            exception,
            "Erro nao tratado. CorrelationId: {CorrelationId}",
            httpContext.TraceIdentifier);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var problemDetails = new ProblemDetails
        {
            Type = "urn:caixa-mercado:erro:erro-inesperado",
            Title = "Ocorreu um erro inesperado.",
            Status = StatusCodes.Status500InternalServerError,
            Detail = "A operacao nao pôde ser concluida.",
            Instance = httpContext.Request.Path
        };
        problemDetails.Extensions["codigo"] = ErrorCodes.UnexpectedError;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        });
    }
}

internal static class ErrorCodes
{
    public const string UnexpectedError = "erro-inesperado";
    public const string ResourceNotFound = "recurso-nao-encontrado";
}
