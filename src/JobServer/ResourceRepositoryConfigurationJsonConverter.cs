using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scrap.Resources;
using Scrap.Resources.FileSystem;

namespace Scrap.JobServer;

public class ResourceRepositoryConfigurationJsonConverter : JsonConverter<IResourceRepositoryConfiguration>
{
    public override void WriteJson(JsonWriter writer, IResourceRepositoryConfiguration value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }

    public override IResourceRepositoryConfiguration ReadJson(JsonReader reader, Type objectType,
        IResourceRepositoryConfiguration existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var cfg = serializer.Deserialize<JObject>(reader);
        cfg.ToObject<FileSystemResourceRepositoryConfiguration>();
        return cfg["Type"].Value<string>() switch
        {
            "filesystem" => cfg.ToObject<FileSystemResourceRepositoryConfiguration>()
                            ?? throw new InvalidOperationException("Couldn't deserialize resource repo config"),
            "list" => cfg.ToObject<ListResourceRepositoryConfiguration>()
                      ?? throw new InvalidOperationException("Couldn't deserialize resource repo config"),
            _ => throw new InvalidOperationException("Couldn't deserialize resource repo config"),
        };
    }
}