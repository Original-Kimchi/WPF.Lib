using System.Net;

namespace WPF.Lib.Api;

public sealed class ApiException : Exception
{
    public ApiException(
        HttpStatusCode statusCode,
        string? responseContent,
        string message)
        : base(message)
    {
        StatusCode = statusCode;
        ResponseContent = responseContent;
    }

    public HttpStatusCode StatusCode { get; }

    public string? ResponseContent { get; }
}
