using System.Net;

namespace WebServer.Tests;

public class HttpClientExtensions
{
    public static async Task<HttpResponseMessage> GetClientRequestResult(Uri uri)
    {
        var client = new HttpClient();
        return await client.GetAsync(uri);
    } 
}