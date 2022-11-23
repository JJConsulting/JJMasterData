using JJMasterData.Web.Models.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JJMasterData.Web.Models;

public class OptionsWriter<T> : IOptionsWriter<T> where T : class, new()
{
    private readonly IOptionsMonitor<T> _options;
    private readonly string _section;
    private readonly string _file;

    public OptionsWriter(
        IOptionsMonitor<T> options,
        string section,
        string file)
    {
        _options = options;
        _section = section;
        _file = file;
    }

    public T Value => _options.CurrentValue;
    public T Get(string name) => _options.Get(name);

    public async Task UpdateAsync(Action<T> applyChanges)
    {

        var jObject = JsonConvert.DeserializeObject<JObject>(await File.ReadAllTextAsync(_file));
        T? sectionObject;
        
        if (jObject.TryGetValue(_section, out JToken section))
        {
            sectionObject = JsonConvert.DeserializeObject<T>(section.ToString());
        }
        else
            sectionObject = Value ?? new T();

        applyChanges(sectionObject);

        jObject[_section] = JObject.Parse(JsonConvert.SerializeObject(sectionObject));
        await File.WriteAllTextAsync(_file, JsonConvert.SerializeObject(jObject, Formatting.Indented));
    }
}