using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace WebServer.Http.Router.Utils;

public static class ControllerBuilder
{
    private delegate T InstanceActivator<T>(params object[] args);
    private static readonly Lazy<ConcurrentDictionary<object, object>> _cachedConstructorsLazy =
        new(() => new ConcurrentDictionary<object, object>());

    private static readonly ConcurrentDictionary<object, object> CachedConstructors = _cachedConstructorsLazy.Value;
    
    public static T Build<T>(Type type)
    {
        if (CachedConstructors.ContainsKey(type))
        {
            var activator = (CachedConstructors[type] as InstanceActivator<T>)!;
            return activator();
        }
        
        var parameters = Expression.Parameter(typeof(object[]), "args");
        var constructor = Expression.New(type);
        var lambdaExpression = Expression.Lambda(typeof(InstanceActivator<T>), constructor, parameters);
        var compiledExpression = (InstanceActivator<T>)lambdaExpression.Compile();
        CachedConstructors[type] = compiledExpression;
        return compiledExpression();
    }
}