using System.Net;
using Newtonsoft.Json;
using WebServer.Controllers.Interfaces;
using WebServer.dto;
using WebServer.Http;
using WebServer.Http.Responses;
using WebServer.Services;

namespace WebServer.Controllers;

public class AuthController : IController
{
    [RouteProps("login", "POST")]
    public async Task Login(HttpListenerContext context)
    {
        using var data = new StreamReader(context.Request.InputStream);
        var userModel = JsonConvert.DeserializeObject<UserLoginDto>(await data.ReadToEndAsync());
        if (userModel != null)
            await AuthService.ServiceInstance.Login(context, userModel);
        else
            await ApiResponse.Send(context, new ErrorEntity(HttpStatusCode.BadGateway, "Something has gone wrong..."));
    }
    
    [RouteProps("logout", "POST", RoutePropsAttribute.UserRole.Authorized)]
    public async Task Logout(HttpListenerContext context)
    {
        using var data = new StreamReader(context.Request.InputStream);
        await AuthService.ServiceInstance.Logout(context);
    }
    
}