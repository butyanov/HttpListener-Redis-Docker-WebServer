using System.Net;
using System.Web;
using ApiConnectionService.Model;
using Newtonsoft.Json;
using WebServer.Enums;

namespace ApiConnectionService;

public static class ApiRequestClient
{
    /*private const string ApiKey = "242d754bc9784983bcc6ec4ca2d84629";*/
    private const string ApiKey = "f75e8523f5154f13b905c2d90fb64bda";
    private const string BaseUri = "https://newsapi.org/v2/top-headlines";
    
    public static async Task<byte[]?> GetApiDataAsync(HttpListenerContext context)
    {
        var data = new StreamReader(context.Request.InputStream);
        var apiRequestConfig = JsonConvert.DeserializeObject<ApiQuery>(await data.ReadToEndAsync());
        if (apiRequestConfig == null) return null;
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "Tabloid.");
        var response = await client.GetAsync(GetFullPath(apiRequestConfig.Category, apiRequestConfig.Country,
            apiRequestConfig.PageSize));
        var content = response.Content;
        return await content.ReadAsByteArrayAsync();

    }

    private static Uri GetFullPath(Category category, Country country, int pageSize)
    {
        var fullPath = new UriBuilder(BaseUri);
        var query = HttpUtility.ParseQueryString(fullPath.Query);
        query["pageSize"] = pageSize is < 1 or > 20 ? "20" : pageSize.ToString();
        query["country"] = country != Country.Default
            ? country.ToString().ToLower()
            : ((Country) new Random(Guid.NewGuid().GetHashCode()).Next(0, Enum.GetNames(typeof(Country)).Length - 2))
            .ToString();
        query["category"] = category != Category.Default
            ? category.ToString().ToLower()
            : ((Category) new Random(Guid.NewGuid().GetHashCode()).Next(0, Enum.GetNames(typeof(Category)).Length - 2))
            .ToString();
        query["apiKey"] = ApiKey;
        fullPath.Query = query.ToString();
        return fullPath.Uri;
    }
}