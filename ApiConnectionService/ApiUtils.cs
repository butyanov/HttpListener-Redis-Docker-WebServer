using System.Reflection;

namespace ApiConnectionService;

public static class ApiUtils
{
    public static Dictionary<string, List<string>> GetFilterProps()
    {
        var propsDict = new Dictionary<string,List<string>>();
        var enums = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsEnum && t.IsPublic);
       foreach (var e in enums)
       {
           var enumNames = e.GetEnumNames().ToList();
           enumNames.Remove("Default");
           propsDict[e.Name] = enumNames;
       }
       
       return propsDict;
    }
}