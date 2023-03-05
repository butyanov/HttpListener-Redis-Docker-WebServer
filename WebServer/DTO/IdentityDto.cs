using Newtonsoft.Json;
using WebServer.Http;

namespace WebServer.DTO;

public class IdentityDto
{
    [JsonProperty("Role")]
    public RoutePropsAttribute.UserRole Role { get; set; }
    
    [JsonProperty("UserId")]
    public Guid UserId { get; set; }
    
    [JsonProperty("UserIP")]
    public string UserIp { get; set; }

    [JsonConstructor]
    public IdentityDto(RoutePropsAttribute.UserRole role, Guid userId, string userIp)
    {
        Role = role;
        UserId = userId;
        UserIp = userIp;
    }
    
    public IdentityDto(string userIp)
    {
        Role = RoutePropsAttribute.UserRole.Guest;
        UserId = Guid.Empty;
        UserIp = userIp;
    }
    
    public IdentityDto(Guid userId, string userIp)
    {
        Role = RoutePropsAttribute.UserRole.Authorized;
        UserId = userId;
        UserIp = userIp;
    }
}