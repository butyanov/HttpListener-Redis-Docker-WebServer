using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace WebServer.Http.Responses;

public static class ApiResponse
{
    public static async Task Send(HttpListenerContext context, object responseBody, HttpStatusCode status = HttpStatusCode.OK, string message = "Response was sent!")
    {
        context.Response.StatusCode = (int) status;
        context.Response.ContentType = "text/plain";
        var encoding = context.Response.ContentEncoding = Encoding.UTF8;
        await context.Response.OutputStream.WriteAsync(encoding.GetBytes(JsonConvert.SerializeObject(responseBody, Formatting.None)));
        Console.WriteLine(message);
    }
    
    public static async Task SendFailure(HttpListenerContext context, ErrorEntity entity)
    {
        await Send(context, entity, entity.Status, entity.Message);
    }
    public static async Task SendSuccess(HttpListenerContext context, SuccessEntity entity)
    {
        await Send(context, entity, message:entity.Message);
    }
}