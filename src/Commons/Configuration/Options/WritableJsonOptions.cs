#nullable enable

using System;
using System.IO;
using System.Threading.Tasks;
using JJMasterData.Commons.Configuration.Options.Abstractions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JJMasterData.Commons.Configuration.Options;

public class WritableJsonOptions<T>(
    IOptionsMonitor<T> options,
    string section,
    string filePath)
    : IWritableOptions<T>
    where T : class, new()
{
    public string FilePath { get; } = filePath;

    public T Value => options.CurrentValue;
    public T Get(string? name) => options.Get(name);

    public async Task UpdateAsync(Action<T> applyChanges)
    {
        T? sectionObject;

        JObject? jObject;

        if (!File.Exists(FilePath))
        {
#if NET
            await using var stream = new StreamWriter(FilePath, true);
#else
            using var stream = new StreamWriter(FilePath, true);
#endif
            await stream.WriteLineAsync("{}");
            await stream.FlushAsync();
            stream.Close();
            jObject = new JObject();
        }
        else
        {
#if NET
            var content = await File.ReadAllTextAsync(FilePath);
#else
            var content = File.ReadAllText(FilePath);
#endif
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
            foreach (var prop in sectionObject.GetType().GetProperties())
            {
                var propString = prop.GetValue(sectionObject)?.ToString();
                if (!string.IsNullOrEmpty(propString))
                {
                    jObject[section]![prop.Name] = propString;
                }
            }

            var serializedObject = JsonConvert.SerializeObject(jObject, Formatting.Indented);

#if NET
            await File.WriteAllTextAsync(FilePath, serializedObject);
#else
            File.WriteAllText(FilePath, serializedObject);
#endif

        }
    }
}