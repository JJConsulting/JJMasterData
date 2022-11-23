using JJMasterData.Web.Models.Abstractions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JJMasterData.Web.Models;

public class OptionsWriter<T> : IOptionsWriter<T> where T : class, new()
{
    private readonly IOptionsMonitor<T> _options;
    private readonly string _section;
    public string FilePath { get; }

    public OptionsWriter(
        IOptionsMonitor<T> options,
        string section,
        string filePath)
    {
        _options = options;
        _section = section;
        FilePath = filePath;
    }

    public T Value => _options.CurrentValue;
    public T Get(string name) => _options.Get(name);

    public async Task UpdateAsync(Action<T> applyChanges)
    {
        var jObject = JsonConvert.DeserializeObject<JObject>(await File.ReadAllTextAsync(FilePath));

        T? sectionObject;
        
        if (jObject!.TryGetValue(_section, out var section))
        {
            sectionObject = JsonConvert.DeserializeObject<T>(section.ToString());
        }
        else
        {
            sectionObject = Value;
        }

        applyChanges(sectionObject!);

        jObject[_section] = JObject.Parse(JsonConvert.SerializeObject(sectionObject));
        await File.WriteAllTextAsync(FilePath, JsonConvert.SerializeObject(jObject, Formatting.Indented));
    }
}