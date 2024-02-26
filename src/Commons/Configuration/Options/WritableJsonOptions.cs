#nullable enable
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using JJMasterData.Commons.Configuration.Options.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JJMasterData.Commons.Configuration.Options;

public class WritableJsonOptions<T>(
    IOptionsMonitor<T> options,
    IMemoryCache memoryCache,
    string section,
    string filePath)
    : IWritableOptions<T>
    where T : class, new()
{
    public string FilePath => filePath;

    public T Value => options.CurrentValue;

    public T Get(string? name) => options.Get(name);

    public async Task UpdateAsync(Action<T> applyChanges)
    {
        T? sectionObject;
        JObject? jObject;

        if (!File.Exists(filePath))
        {
            using var stream = new StreamWriter(filePath, true);
            await stream.WriteLineAsync("{}");
            await stream.FlushAsync();
            jObject = new JObject();
        }
        else
        {
            var content = File.ReadAllText(filePath);
            jObject = JsonConvert.DeserializeObject<JObject>(content);
        }

        if (jObject != null && jObject.TryGetValue(section, out var value))
        {
            sectionObject = JsonConvert.DeserializeObject<T>(value.ToString());
        }
        else
        {
            sectionObject = Value;
        }

        sectionObject ??= new T();

        applyChanges(sectionObject);

        if (jObject != null)
        {
            var properties = GetCachedProperties(sectionObject.GetType());

            foreach (var prop in properties)
            {
                var propString = prop.GetValue(sectionObject)?.ToString();
                if (!string.IsNullOrEmpty(propString) && Type.GetTypeCode(prop.PropertyType) != TypeCode.Object)
                {
                    jObject[section]![prop.Name] = propString;
                }
            }

            var serializedObject = JsonConvert.SerializeObject(jObject, Formatting.Indented);
            File.WriteAllText(filePath, serializedObject);
        }
    }

    private PropertyInfo[] GetCachedProperties(Type type)
    {
        if (memoryCache.TryGetValue(type, out PropertyInfo[] cachedProperties))
        {
            return cachedProperties;
        }

        var properties = type.GetProperties();
        
        memoryCache.Set(type, properties);

        return properties;
    }
}