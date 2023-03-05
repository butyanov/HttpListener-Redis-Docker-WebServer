using System.Net;

namespace WebServer.Http.Responses;

public struct ErrorEntity
{
    public HttpStatusCode Status { get; }
    public string Message { get; }

    public ErrorEntity(HttpStatusCode status, string message)
    {
        Status = status;
        Message = message;
    }
}