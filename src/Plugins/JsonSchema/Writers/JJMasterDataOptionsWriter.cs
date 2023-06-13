using JJMasterData.Commons.Options;
using Newtonsoft.Json.Schema;

namespace JJMasterData.JsonSchema.Writers;

public class JJMasterDataOptionsWriter : BaseWriter
{
    public override async Task WriteAsync()
    {
        var schema = new JSchema
        {
            Title = "JJMasterData",
            Description = "JSON schema for JJMasterData ASP.NET Core's appsettings.json file"
        };

        var jjmasterdata = new KeyValuePair<string, JSchema>("JJMasterData", Generator.Generate(typeof(JJMasterDataCommonsOptions)));

        schema.Properties.Add(jjmasterdata);

        await WriteSchemaAsync("JJMasterDataOptions", schema);
    }
}