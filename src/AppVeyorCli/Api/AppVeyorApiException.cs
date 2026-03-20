using System.Net;

namespace AppVeyorCli.Api;

public class AppVeyorApiException(HttpStatusCode statusCode, string message)
    : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}
