using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Scrap.Resources;
using Scrap.Resources.FileSystem;

namespace API
{
    public class ResourceRepositoryConfigurationJsonConverter : JsonConverter<IResourceRepositoryConfiguration>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(IResourceRepositoryConfiguration).IsAssignableFrom(typeToConvert);
        }

        public override IResourceRepositoryConfiguration Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<FileSystemResourceRepositoryConfiguration>(
                ref reader,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }

        public override void Write(Utf8JsonWriter writer, IResourceRepositoryConfiguration value, JsonSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNullValue();
                return;
            }

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