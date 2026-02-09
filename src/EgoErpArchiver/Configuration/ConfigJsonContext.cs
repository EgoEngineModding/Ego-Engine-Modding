using System.Text.Json.Serialization;

namespace EgoErpArchiver.Configuration;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(SettingsConfig))]
public partial class ConfigJsonContext : JsonSerializerContext;
