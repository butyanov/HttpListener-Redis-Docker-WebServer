using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.RegularExpressions;

namespace WebServer.Services;

public static class ValidationService
{
    public static List<ValidationResult>? ValidateModel<T>(T model)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));
        var result = new List<ValidationResult>();
        var context = new ValidationContext(model);
        return Validator.TryValidateObject(model, context, result, true) ? null : result;
    }
    
    public static string ValidateClientIp(HttpListenerContext context)
    {
        var ip = context.Request.RemoteEndPoint.ToString();
        return Regex.Replace(ip.Contains("[::1]") 
            ? ip.Replace("[::1]", "127.0.0.1")
            : ip, @":\d+", "");
    }
}