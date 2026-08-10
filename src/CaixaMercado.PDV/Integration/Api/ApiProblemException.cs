using System.Net;

namespace CaixaMercado.PDV.Integration.Api;

public sealed class ApiProblemException : Exception
{
    public ApiProblemException(HttpStatusCode statusCode, string codigo, string mensagem, string? correlationId = null)
        : base(mensagem)
    {
        StatusCode = statusCode;
        Codigo = codigo;
        CorrelationId = correlationId;
    }

    public HttpStatusCode StatusCode { get; }
    public string Codigo { get; }
    public string? CorrelationId { get; }
}
