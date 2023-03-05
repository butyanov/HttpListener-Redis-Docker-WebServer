using System.Net;
using System.Reflection;
using Newtonsoft.Json;
using Redis;
using StackExchange.Redis;
using WebServer.Controllers.Interfaces;
using WebServer.DTO;
using WebServer.Http.Responses;
using WebServer.Http.Router.Utils;
using WebServer.Services;

namespace WebServer.Http.Router;

internal static class Router
{
    private static IEnumerable<MethodInfo> Methods =>
        Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(type => typeof(IController)
                .IsAssignableFrom(type))
            .SelectMany(type => type.GetMethods()
                .Where(method => method.GetCustomAttributes(true).Any(attr => attr is RoutePropsAttribute)));
    private static MethodInfo DefaultMethod => 
        Methods.SingleOrDefault(method => method
        .GetCustomAttributes(true)
        .Any(y => y is RoutePropsAttribute {Method: "*", Route: "*"}))!;
    
    public static async Task GetRoute(HttpListenerContext context)
    {
        var access = await IdentifyClientRights(context);
        var method = GetConcreteMethod(context);
        if (method is not null)
        {
            if (method.Name.ToLower() is "login" && access.Contains(RoutePropsAttribute.UserRole.Authorized))
                await ApiResponse.SendFailure(context, new ErrorEntity(HttpStatusCode.BadRequest, "You are already authorized."));
            else
            {
                var methodAttrs = method.GetCustomAttributes(true);
                if (methodAttrs.Any(attr =>
                        attr is RoutePropsAttribute routeProps && !access.Contains(routeProps.Access)))
                {
                    if (!access.Contains(RoutePropsAttribute.UserRole.Guest))
                        await ApiResponse.SendFailure(context,
                            new ErrorEntity(HttpStatusCode.Forbidden, "Not enough rights."));
                    else
                        await ApiResponse.SendFailure(context,
                            new ErrorEntity(HttpStatusCode.Unauthorized, "Login please."));
                }
                else
                    await ((Task)method.Invoke(ControllerBuilder.Build<IController>(method.DeclaringType!),
                        new object[] { context })!).ConfigureAwait(false);
            }
        }        
        else
            await ((Task)DefaultMethod.Invoke(ControllerBuilder.Build<IController>(DefaultMethod.DeclaringType!),
                new object[] {context})!).ConfigureAwait(false);
    }
    
    private static async Task<List<RoutePropsAttribute.UserRole>> IdentifyClientRights(HttpListenerContext context)
    {
        var access = new List<RoutePropsAttribute.UserRole>{RoutePropsAttribute.UserRole.Anonymous};
        var requestCookie = context.Request.Cookies["session-id"];
        if (requestCookie is null) return access;
        
        var redisStorage = RedisStorage.RedisConnection;
        if (requestCookie.Expired)
        {
            await redisStorage.KeyDeleteAsync(new RedisKey(requestCookie.Value));
            return access;
        }
        
        var sessionDataString = await redisStorage.StringGetAsync(requestCookie.Value);
        var sessionData = JsonConvert.DeserializeObject<IdentityDto>(sessionDataString!);
        if (ValidationService.ValidateClientIp(context) != sessionData!.UserIp) return access;
        
        access.Add(RoutePropsAttribute.UserRole.Guest);
        var responseCookies = context.Response.Cookies;
        if (sessionData.Role is RoutePropsAttribute.UserRole.Guest)
        {
            requestCookie.Expires = DateTime.UtcNow.AddMinutes(30d);
            responseCookies.Add(requestCookie);
            return access;
        }
        
        requestCookie.Expires = DateTime.UtcNow.AddDays(14d);
        responseCookies.Add(requestCookie);
        access.Add(RoutePropsAttribute.UserRole.Authorized);
        context.Request.Headers.Add("user-id", sessionData.UserId.ToString());
        return access;
    }
    
    private static MethodInfo? GetConcreteMethod(HttpListenerContext context) => 
        Methods.SingleOrDefault(x =>
        {
            var route = context.Request.Url?.Segments[1].Replace("/", "");
            return x.GetCustomAttributes(true).Any(y =>
                y is RoutePropsAttribute mapping && mapping.Method == context.Request.HttpMethod && mapping.Route == route);
        });
}