using JJMasterData.Web.Models.Abstractions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JJMasterData.Web.Models;

public class WritableOptions<T> : IWritableOptions<T> where T : class, new()
{
    private readonly IOptionsMonitor<T> _options;
    private readonly string _section;
    public string FilePath { get; }

    public WritableOptions(
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
        T? sectionObject;

        JObject? jObject;
        
        if (!File.Exists(FilePath))
        {
            await using var stream = new StreamWriter(FilePath, true);
            await stream.WriteLineAsync("{}");
            await stream.FlushAsync();
            stream.Close();
            jObject = new JObject();
        }
        else
        {
            jObject = JsonConvert.DeserializeObject<JObject>(await File.ReadAllTextAsync(FilePath));
        }
        
        if (jObject != null && jObject.TryGetValue(_section, out var section))
        {
            sectionObject = JsonConvert.DeserializeObject<T>(section.ToString());
        }
        else
        {
            sectionObject = Value;
        }

        applyChanges(sectionObject!);

        if (jObject != null)
        {
            jObject[_section] = JObject.Parse(JsonConvert.SerializeObject(sectionObject));

            await File.WriteAllTextAsync(FilePath, JsonConvert.SerializeObject(jObject, Formatting.Indented));
        }
    }
}