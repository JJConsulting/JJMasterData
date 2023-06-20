#nullable enable

using System;
using System.IO;
using System.Threading.Tasks;
using JJMasterData.Commons.Configuration.Options.Abstractions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JJMasterData.Commons.Configuration.Options;

public class WritableJsonOptions<T> : IWritableOptions<T> where T : class, new()
{
    private readonly IOptionsMonitor<T> _options;
    private readonly string _section;
    public string FilePath { get; }

    public WritableJsonOptions(
        IOptionsMonitor<T> options,
        string section,
        string filePath)
    {
        _options = options;
        _section = section;
        FilePath = filePath;
    }

    public T Value => _options.CurrentValue;
    public T Get(string? name) => _options.Get(name);

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

        if (jObject != null && jObject.TryGetValue(_section, out var section))
        {
            sectionObject = JsonConvert.DeserializeObject<T>(section.ToString());
        }
        else
        {
            sectionObject = Value;
        }

        sectionObject ??= new T();

        applyChanges(sectionObject);

        if (jObject != null)
        {
            jObject[_section] = JObject.Parse(JsonConvert.SerializeObject(sectionObject));

            var serializedObject = JsonConvert.SerializeObject(jObject, Formatting.Indented);

#if NET
            await File.WriteAllTextAsync(FilePath, serializedObject);
#else
            File.WriteAllText(FilePath, serializedObject);
#endif

        }
    }
}