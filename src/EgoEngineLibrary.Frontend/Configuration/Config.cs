using System.Collections.Concurrent;

namespace EgoEngineLibrary.Frontend.Configuration;

public static class Config
{
    private static readonly ConcurrentDictionary<Type, ConfigInfo> ConfigInfos = new();

    public static void Add<T>(IConfigProvider provider)
    {
        if (!ConfigInfos.TryAdd(typeof(T), new ConfigInfo { Provider = provider }))
        {
            throw new InvalidOperationException($"The configuration provider for type {typeof(T)} already exists.");
        }
    }

    public static void LoadAll()
    {
        foreach (var configType in ConfigInfos.Keys)
        {
            _ = Load(configType);
        }
    }
    
    public static T Load<T>(bool reload = false)
    {
        return (T)Load(typeof(T), reload);
    }
    
    public static object Load(Type configType, bool reload = false)
    {
        if (!ConfigInfos.TryGetValue(configType, out var configInfo))
        {
            throw new InvalidOperationException($"The configuration provider for type {configType} does not exist.");
        }

        lock (configInfo.Lock)
        {
            if (!reload && configInfo.Value is not null)
            {
                return configInfo.Value;
            }

            var value = configInfo.Provider.Load();
            configInfo.Value = value;
            return value;
        }
    }

    public static void Save<T>()
    {
        if (!ConfigInfos.TryGetValue(typeof(T), out var configInfo))
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
