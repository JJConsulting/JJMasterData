using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JJMasterData.Web.Models;
public interface IWritableOptions<out T> : IOptionsSnapshot<T> where T : class, new()
{
    void Update(Action<T> applyChanges);
}

public class WritableOptions<T> : IWritableOptions<T> where T : class, new()
{
    private readonly IOptionsMonitor<T> _options;
    private readonly string _section;
    private readonly string _file;

    public WritableOptions(
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

    public void Update(Action<T> applyChanges)
    {

        var jObject = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(_file));
        T? sectionObject;
        
        if (jObject.TryGetValue(_section, out JToken section))
        {
            sectionObject = JsonConvert.DeserializeObject<T>(section.ToString());
        }
        else
            sectionObject = Value ?? new T();

        applyChanges(sectionObject);

        jObject[_section] = JObject.Parse(JsonConvert.SerializeObject(sectionObject));
        File.WriteAllText(_file, JsonConvert.SerializeObject(jObject, Formatting.Indented));
    }
}

public static class ServiceCollectionExtensions
{
    public static void ConfigureWritable<T>(
        this IServiceCollection services,
        IConfigurationSection section,
        string file = "appsettings.json") where T : class, new()
    {
        services.Configure<T>(section);
        services.AddTransient<IWritableOptions<T>>(provider =>
        {
            var options = provider.GetService<IOptionsMonitor<T>>();
            return new WritableOptions<T>(options, section.Key, file);
        });
    }
}
