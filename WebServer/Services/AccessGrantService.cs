using System.Net;
using Newtonsoft.Json;
using Redis;
using StackExchange.Redis;
using WebServer.DTO;

namespace WebServer.Services;

public class AccessGrantService
{
    private static readonly Lazy<AccessGrantService> _service = new(() => new AccessGrantService());
    public static AccessGrantService ServiceInstance => _service.Value;
    private AccessGrantService()
    {
    }

    public async Task SetGuestSession(HttpListenerContext context)
    {
        
        var sessionId = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid() + "session");
        var sessionData = new IdentityDto(ValidationService.ValidateClientIp(context));
        var sessionDataJson = JsonConvert.SerializeObject(sessionData);
            await RedisStorage.RedisConnection.StringSetAsync(new RedisKey(sessionId), new RedisValue(sessionDataJson));
        context.Response.Cookies.Add(new Cookie
        {
            Name = "session-id",
            Value = sessionId,
            Expires = DateTime.UtcNow.AddMinutes(30d)
        });
    }
    
    public async Task SetUserSession(HttpListenerContext context, Guid userId)
    {
        var requestCookie = context.Request.Cookies["session-id"];
        var sessionDataJson = JsonConvert.SerializeObject(new IdentityDto(userId, ValidationService.ValidateClientIp(context)));
        await RedisStorage.RedisConnection.StringSetAsync(new RedisKey(requestCookie!.Value), new RedisValue(sessionDataJson));
        requestCookie.Expires = DateTime.Now.AddDays(14d);
        context.Response.Cookies.Add(requestCookie);
    }
}