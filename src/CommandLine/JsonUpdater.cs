using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scrap.Domain.Resources.FileSystem;

namespace Scrap.CommandLine;

public class JsonUpdater
{
    private readonly IFileSystem _fileSystem;
    private readonly string _jsonFilePath;

    public JsonUpdater(IFileSystem fileSystem, string jsonFilePath)
    {
        _fileSystem = fileSystem;
        _jsonFilePath = jsonFilePath;
    }

    public async Task AddOrUpdateAsync(IEnumerable<KeyValuePair<string, object?>> updates)
    {
        try
        {
            var filePath = _fileSystem.Path.Combine(AppContext.BaseDirectory, _jsonFilePath);
            var json = await _fileSystem.File.ReadAllTextAsync(filePath);
            var jsonObj = JObject.Parse(json);
            var rootProp = new JProperty("root", jsonObj);
            foreach (var (sectionPathKey, value) in updates)
            {
                SetValue(sectionPathKey, rootProp, value);
            }

            var output = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
            await _fileSystem.File.WriteAllTextAsync(filePath, output);
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
        var targetProp = split.Aggregate(rootProp, AccumulateProperty);

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
