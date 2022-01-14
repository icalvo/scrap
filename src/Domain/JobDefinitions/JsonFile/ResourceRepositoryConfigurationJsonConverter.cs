using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Scrap.Resources;
using Scrap.Resources.FileSystem;

namespace Scrap.JobDefinitions.JsonFile
{
    public class ResourceRepositoryConfigurationJsonConverter : JsonConverter<IResourceRepositoryConfiguration>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(IResourceRepositoryConfiguration).IsAssignableFrom(typeToConvert);
        }

        public override IResourceRepositoryConfiguration Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var cfg = JsonSerializer.Deserialize<JsonElement>(
                ref reader,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var json = cfg.GetRawText();
            return cfg.GetProperty("type").GetString() switch
            {
                "filesystem" => JsonSerializer.Deserialize<FileSystemResourceRepositoryConfiguration>(
                                    json,
                                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
                                ?? throw new InvalidOperationException("Couldn't deserialize resource repo config"),
                "list" => JsonSerializer.Deserialize<ListResourceRepositoryConfiguration>(
                              json,
                              new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
                          ?? throw new InvalidOperationException("Couldn't deserialize resource repo config"),
                _ => throw new InvalidOperationException("Couldn't deserialize resource repo config"),
            };
        }

        public override void Write(Utf8JsonWriter writer, IResourceRepositoryConfiguration value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            foreach (var property in value.GetType().GetProperties())
            {
                if (!property.CanRead)
                    continue;
                var propertyValue = property.GetValue(value);
                writer.WritePropertyName(property.Name);
                JsonSerializer.Serialize(writer, propertyValue, options);
            }
            writer.WriteEndObject();
        }
    }
}