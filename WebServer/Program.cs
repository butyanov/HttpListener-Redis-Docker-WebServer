using System.Net;
using WebServer.Http.Router;

await EntryPoint();

static async Task EntryPoint()
{
    var listener = new HttpListener();
    listener.Prefixes.Add("http://localhost:8080/");
    listener.Start();
    while (listener.IsListening)
    {
        var context = await listener.GetContextAsync();
        _ = Task.Run(async () =>
        {
            await Router.GetRoute(context);
            context.Response.OutputStream.Close();
            context.Response.Close();
        });
    }

    listener.Stop();
    listener.Close();
}
