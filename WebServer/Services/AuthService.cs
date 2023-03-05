using System.Net;
using Newtonsoft.Json;
using Redis;
using StackExchange.Redis;
using WebServer.dto;
using WebServer.DTO;
using WebServer.Http;
using WebServer.Http.Responses;
using WebServer.Repositories;

namespace WebServer.Services;

public class AuthService
{
    private static readonly Lazy<AuthService> _service = new(()=> new AuthService());
    public static AuthService ServiceInstance => _service.Value;
    private AuthService()
    {
    }
    
    public async Task Login(HttpListenerContext context, UserLoginDto model)
    {
        var validationResult = ValidationService.ValidateModel(model);
        if (validationResult != null)
            await ApiResponse.SendFailure(context, new ErrorEntity(HttpStatusCode.BadRequest, $"{string.Join(" ",validationResult)}"));
        else
        {
            var receivedUser = await UsersRepository.RepositoryInstance.Get(model.Email);
            if (receivedUser == null)
                await ApiResponse.SendFailure(context, new ErrorEntity(HttpStatusCode.BadRequest, "User was not found!"));
            else
            {
                if (BCrypt.Net.BCrypt.Verify(model.Password, receivedUser.Password))
                {
                    await AccessGrantService.ServiceInstance.SetUserSession(context, receivedUser.Id);
                    await ApiResponse.SendSuccess(context,
                        new SuccessEntity(new UserDto(receivedUser), "You've been successfully authorized!"));
                }
                else
                    await ApiResponse.SendFailure(context, new ErrorEntity(HttpStatusCode.BadRequest, "Incorrect password!"));
            }
        }
    }
    
    public async Task Logout(HttpListenerContext context)
    {
        var requestCookie = context.Request.Cookies["session-id"];
        var sessionDataJson = JsonConvert.SerializeObject(new IdentityDto(ValidationService.ValidateClientIp(context))); 
        await RedisStorage.RedisConnection.StringSetAsync(new RedisKey(requestCookie?.Value), new RedisValue(sessionDataJson));
        requestCookie!.Expires = DateTime.Now.AddMinutes(30d);
        context.Response.Cookies.Add(requestCookie);
        await ApiResponse.SendSuccess(context, new SuccessEntity(null, "You've been successfully logged out!"));
    }

    public async Task<bool> IsAuthorized(HttpListenerContext context)
    {
        var requestCookie = context.Request.Cookies["session-id"];
        if (requestCookie == null)
            return false;
        var sessionDataString = await RedisStorage.RedisConnection.StringGetAsync(requestCookie.Value);
        var sessionData = JsonConvert.DeserializeObject<IdentityDto>(sessionDataString!);
        return sessionData!.Role == RoutePropsAttribute.UserRole.Authorized;
    }
}