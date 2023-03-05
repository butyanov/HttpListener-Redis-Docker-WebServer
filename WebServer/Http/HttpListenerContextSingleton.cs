using System.Net;

namespace WebServer.Http;

public class HttpListenerSingleton
{
    private static readonly Lazy<Task<HttpListenerSingleton>> LazySingleton = new(async() => await Task.Run(CreateSingleton));
    public static Task<HttpListenerSingleton> HttpListenerInstance => LazySingleton.Value;
    public HttpListenerContext Context { get; private set; }
    public HttpListener Listener { get; }

    private HttpListenerSingleton(HttpListener listener)
    {
        Listener = listener;
    } 
    
    private static HttpListenerSingleton CreateSingleton()
    {
        var listener = new HttpListener();
        return new HttpListenerSingleton(listener);
    }
    
    public async Task DeclareContextAsync()
    {
        Context = await Listener.GetContextAsync();
    }
}
