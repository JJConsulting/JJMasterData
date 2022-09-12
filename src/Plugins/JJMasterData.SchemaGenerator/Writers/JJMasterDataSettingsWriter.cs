using JJMasterData.Commons.Logging;
using JJMasterData.Commons.Settings;
using Newtonsoft.Json.Schema;

namespace JJMasterData.SchemaGenerator.Writers;

public class JJMasterDataSettingsWriter : BaseWriter
{
    public async Task WriteAsync()
    {
        
        var schema = new JSchema
        {
            Title = "JJMasterData",
            Description = "JSON schema for JJMasterData ASP.NET Core's appsettings.json file"
        };

        var jjmasterdata = new KeyValuePair<string, JSchema>("JJMasterData", Generator.Generate(typeof(JJMasterDataSettings)));

        var logger = new KeyValuePair<string, JSchema>("Logger", Generator.Generate(typeof(LoggerSettings)));

        var swagger = new KeyValuePair<string, JSchema>("Swagger", Generator.Generate(typeof(Swagger)));

        var theme = new KeyValuePair<string, JSchema>("Theme", Generator.Generate(typeof(string)));

        jjmasterdata.Value.Properties.Add(logger);
        jjmasterdata.Value.Properties.Add(swagger);
        jjmasterdata.Value.Properties.Add(theme);

        jjmasterdata.Value.Required.Clear();

        schema.Properties.Add(jjmasterdata);

        await File.WriteAllTextAsync(GetFilePath("jjmasterdata"), schema.ToString());
        
        Console.WriteLine("File successfuly generated!\n");
        
    }
}