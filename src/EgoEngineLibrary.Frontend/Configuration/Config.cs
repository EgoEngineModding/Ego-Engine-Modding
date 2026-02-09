using System.Collections.Concurrent;

namespace EgoEngineLibrary.Frontend.Configuration;

public class Config
{
    private static readonly ConcurrentDictionary<Type, ConfigInfo> _configInfos = new();

    public static void Add<T>(IConfigProvider provider)
    {
        if (!_configInfos.TryAdd(typeof(T), new ConfigInfo { Provider = provider }))
        {
            throw new InvalidOperationException($"The configuration provider for type {typeof(T)} already exists.");
        }
    }
    
    public static T Load<T>(bool reload = false)
    {
        if (!_configInfos.TryGetValue(typeof(T), out var configInfo))
        {
            throw new InvalidOperationException($"The configuration provider for type {typeof(T)} does not exist.");
        }

        lock (configInfo.Lock)
        {
            if (!reload && configInfo.Value is not null)
            {
                return (T)configInfo.Value;
            }

            var value = (T)configInfo.Provider.Load();
            configInfo.Value = value;
            return value;
        }
    }

    public static void Save<T>()
    {
        if (!_configInfos.TryGetValue(typeof(T), out var configInfo))
        {
            throw new InvalidOperationException($"The configuration provider for type {typeof(T)} does not exist.");
        }

        lock (configInfo.Lock)
        {
            if (configInfo.Value is not null)
            {
                configInfo.Provider.Save(configInfo.Value);
            }
        }
    }

    private class ConfigInfo
    {
        public object? Value { get; set; }
        
        public required IConfigProvider Provider { get; init; }

        public Lock Lock { get; } = new();
    }
}
