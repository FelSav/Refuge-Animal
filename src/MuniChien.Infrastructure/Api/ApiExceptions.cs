using System.Net;

namespace MuniChien.Infrastructure.Api;

/// <summary>Le serveur est injoignable ou la requête a dépassé son délai.</summary>
public sealed class ApiConnectionException : Exception
{
    public ApiConnectionException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>Le serveur a répondu avec un code HTTP non réussi (401, 404, 500, etc.).</summary>
public sealed class ApiHttpException : Exception
{
    public ApiHttpException(string endpoint, HttpStatusCode statusCode)
        : base($"L'API a retourné HTTP {(int)statusCode} ({statusCode}) pour {endpoint}.")
    {
        Endpoint = endpoint;
        StatusCode = statusCode;
    }

    public string Endpoint { get; }
    public HttpStatusCode StatusCode { get; }
}

/// <summary>Le serveur a répondu, mais le contenu n'est pas du JSON valide.</summary>
public sealed class ApiResponseException : Exception
{
    public ApiResponseException(string message, Exception innerException)
        : base(message, innerException) { }
}
