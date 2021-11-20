using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Scrap.JobDefinitions.JsonFile
{
    public class TimeSpanJsonConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) =>
            TimeSpan.ParseExact(reader.GetString() ?? "",
                "c", CultureInfo.InvariantCulture);

        public override void Write(
            Utf8JsonWriter writer,
            TimeSpan timeSpan,
            JsonSerializerOptions options) =>
            writer.WriteStringValue(timeSpan.ToString(
                "c", CultureInfo.InvariantCulture));
    }
}