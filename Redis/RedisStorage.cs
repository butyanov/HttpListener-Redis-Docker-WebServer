using StackExchange.Redis;

namespace Redis;

public class RedisStorage
{
    private static readonly Lazy<ConnectionMultiplexer> LazyConnection = new(
        () => ConnectionMultiplexer.Connect(new ConfigurationOptions 
        { 
            EndPoints =
            {
                "localhost:6379"
            },
            Password = "5eb105669659490713a9bcd1482ba43283ad9303b2a09af166bd09ace4bda7e6"
        }));
    public static IDatabase RedisConnection => LazyConnection.Value.GetDatabase();
}