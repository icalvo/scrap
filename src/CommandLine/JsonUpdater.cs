using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Scrap.CommandLine;

public class JsonUpdater
{
    private readonly string _jsonFilePath;

    public JsonUpdater(string jsonFilePath)
    {
        _jsonFilePath = jsonFilePath;
    }

    public void AddOrUpdate(IEnumerable<KeyValuePair<string, object?>> updates)
    {
        try
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, _jsonFilePath);
            var json = File.ReadAllText(filePath);
            var jsonObj = JObject.Parse(json);
            var rootProp = new JProperty("root", jsonObj);
            foreach (var (sectionPathKey, value) in updates)
            {
                SetValue(sectionPathKey, rootProp, value);
            }

            var output =
                JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
            File.WriteAllText(filePath, output);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error writing app settings | {ex.Message}", ex);
        }
    }

    private static void SetValue(string sectionPathKey, JProperty rootProp, object? value)
    {
        if (string.IsNullOrWhiteSpace(sectionPathKey))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(sectionPathKey));
        }

        var split = sectionPathKey.Split(":");
        var targetProp = split
            .Aggregate(rootProp, AccumulateProperty);

        targetProp.Value = new JValue(value);
    }

    private static JProperty AccumulateProperty(JProperty jsonProp, string propertyName) =>
        jsonProp.Value is JObject obj
            ? EnsureObjectProperty(obj, propertyName)
            : throw new Exception("One of the sections is not a JSON object.");

    private static JProperty EnsureObjectProperty(JObject jsonObj, string propertyName)
    {
        var prop = jsonObj.Property(propertyName);
        if (prop != null)
        {
            return prop;
        }

        prop = new JProperty(propertyName, new JObject());
        jsonObj.Add(prop);

        return prop;
    }
}
