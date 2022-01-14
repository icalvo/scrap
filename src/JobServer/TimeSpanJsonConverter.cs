using System;
using System.Globalization;
using Newtonsoft.Json;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace Scrap.JobServer;

public class TimeSpanJsonConverter : JsonConverter<TimeSpan>
{
    public override void WriteJson(JsonWriter writer, TimeSpan value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString(
            "c", CultureInfo.InvariantCulture));
    }

    public override TimeSpan ReadJson(JsonReader reader, Type objectType, TimeSpan existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        return TimeSpan.ParseExact(reader.ReadAsString() ?? "",
            "c", CultureInfo.InvariantCulture);
    }
}