using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using WebServer.Enums;

namespace ApiConnectionService.Model;

public class ApiQuery
{
    [JsonConverter(typeof(StringEnumConverter))]
    public Country Country { get; set; }
    
    [JsonConverter(typeof(StringEnumConverter))]
    public Category Category { get; set; }
    public int PageSize { get; set; }
}