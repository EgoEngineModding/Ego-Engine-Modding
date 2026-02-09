using System.Text.Json.Serialization;

namespace EgoErpArchiver.Configuration;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(AppSettings))]
public partial class ConfigJsonContext : JsonSerializerContext;
