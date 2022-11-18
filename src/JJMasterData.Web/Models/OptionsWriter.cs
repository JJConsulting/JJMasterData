using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JJMasterData.Web.Models;

public interface IOptionsWriter
{
    public void UpdateOptions(Action<JObject> callback, bool reload = true);
}

public class OptionsWriter : IOptionsWriter 
{
    private readonly IHostingEnvironment _environment;
    private readonly IConfigurationRoot _configuration;
    private readonly string _file;

    public OptionsWriter(
        IHostingEnvironment environment, 
        IConfigurationRoot configuration, 
        string file)
    {
        _environment = environment;
        _configuration = configuration;
        _file = file;
    }

    public void UpdateOptions(Action<JObject> callback, bool reload = true)
    {
        
        JObject? config = JsonConvert
            .DeserializeObject<JObject>(File.ReadAllText(_file));
        
        callback(config);
        
        File.WriteAllText(_file, JsonConvert.SerializeObject(config, Formatting.Indented));

        _configuration.Reload();
    }
}