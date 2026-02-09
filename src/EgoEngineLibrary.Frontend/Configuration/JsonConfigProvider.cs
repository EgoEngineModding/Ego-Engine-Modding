using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace EgoEngineLibrary.Frontend.Configuration;

public class JsonConfigProvider : IConfigProvider
{
    private readonly string _filePath;
    private readonly JsonTypeInfo _options;

    public JsonConfigProvider(string filePath, JsonTypeInfo options)
    {
        _filePath = filePath;
        _options = options;
    }
    
    public object Load()
    {
        using Stream stream = File.Exists(_filePath)
            ? File.Open(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read)
            : new MemoryStream("{}"u8.ToArray());
        
        var res = JsonSerializer.Deserialize(stream, _options);
        if (res is null)
        {
            throw new JsonException("Object is null");
        }
        
        return res;
    }

    public void Save(object config)
    {
        using var fs = File.Open(_filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
        JsonSerializer.Serialize(fs, config, _options);
    }
}
