namespace WebServer.Http;

[AttributeUsage(AttributeTargets.Method)]
public class RoutePropsAttribute : Attribute
{
    public string Route;
    public string Method;
    public UserRole Access;

    public RoutePropsAttribute(string route, string method, UserRole access = UserRole.Guest)
    {
        Route = route;
        Method = method;
        Access = access;
    }
    
    public enum UserRole
    {
        Anonymous,
        Guest,
        Authorized
    } 
}