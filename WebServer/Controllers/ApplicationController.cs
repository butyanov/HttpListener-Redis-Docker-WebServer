using System.Net;
using System.Text;
using ApiConnectionService;
using WebServer.Controllers.Interfaces;
using WebServer.Enums;
using WebServer.Http;
using WebServer.Http.Responses;
using WebServer.Http.Responses.Types;
using WebServer.Services;

namespace WebServer.Controllers;

public class ApplicationController : IController
{
    [RouteProps("*", "*", RoutePropsAttribute.UserRole.Anonymous)]
    public async Task ShowDefault(HttpListenerContext context)
    {
        var path = context.Request.Url?.LocalPath
            .Split("/")
            .Skip(1)
            .ToArray();
        var response = context.Response;
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "public");
        if (path != null)
            for (var i = 0; i < path.Length - 1; i++)
                basePath = Path.Combine(basePath, $@"{path[i]}/");

        basePath = Path.Combine(basePath, path?[^1] ?? throw new FileNotFoundException());
        if (File.Exists(basePath))
        {
            var acceptedExtensions = new List<string> { ".css", ".js", ".jpg", ".jpeg" };
            if (!acceptedExtensions.Contains(Path.GetExtension(basePath))) 
                await ApiResponse.Send(context, new ErrorEntity(HttpStatusCode.Forbidden, "Not enough rights."));
            else
                await ShowFile(basePath, context);
        }
        else
            await ApiResponse.Send(context, new ErrorEntity(HttpStatusCode.NotFound, "Page wasn't found."));
    }

    [RouteProps("home", "GET", RoutePropsAttribute.UserRole.Anonymous)]
    public async Task ShowIndex(HttpListenerContext context)
    {
       if(await AuthService.ServiceInstance.IsAuthorized(context))
           await ShowFile(@"public/UserIndex.html", context);
       else
           await ShowFile(@"public/GuestIndex.html", context);
    }
    
    [RouteProps("article", "GET", RoutePropsAttribute.UserRole.Anonymous)]
    public async Task ShowArticle(HttpListenerContext context)
    {
        await ShowFile(@"public/article.html", context);
    }
    
    [RouteProps("auth", "GET")]
    public async Task ShowAuthPage(HttpListenerContext context)
    {
        await ShowFile(@"public/auth.html", context);
    }
    
    [RouteProps("account", "GET", RoutePropsAttribute.UserRole.Authorized)]
    public async Task ShowAccountPage(HttpListenerContext context)
    {
        await ShowFile(@"public/account.html", context);
    }
    
    [RouteProps("api", "POST", RoutePropsAttribute.UserRole.Anonymous)]
    public static async Task ShowApi(HttpListenerContext context)
    {
        var file = await ApiRequestClient.GetApiDataAsync(context);
        if (file == null)
            await ApiResponse.SendFailure(context, new ErrorEntity(HttpStatusCode.BadRequest, "Api были поданы некорректные данные."));
        else
        {
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.ContentType = "application/json";
            context.Response.ContentEncoding = Encoding.UTF8;
            await context.Response.OutputStream.WriteAsync(file);
        }
    }
    [RouteProps("filterProps", "GET")]
    public static async Task GetFilterProps(HttpListenerContext context)
    {
        await ApiResponse.Send(context, ApiUtils.GetFilterProps());
    }
    
    private async Task ShowFile(string path, HttpListenerContext context)
    {
        if (context.Request.Cookies["session-id"] is null)
            await AccessGrantService.ServiceInstance.SetGuestSession(context);

        var file = await File.ReadAllBytesAsync(path);
        context.Response.StatusCode = (int) HttpStatusCode.OK;
        context.Response.ContentType = Path.GetExtension(path) switch
        {
            ".js" => ContentType.Js,
            ".css" => ContentType.Css,
            ".html" => ContentType.Html,
            _ => ContentType.TextPlain
        };
        await context.Response.OutputStream.WriteAsync(file);
    }
}