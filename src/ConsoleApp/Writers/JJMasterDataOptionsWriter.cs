using JJMasterData.Commons.Configuration.Options;
using Newtonsoft.Json.Schema;

namespace JJMasterData.ConsoleApp.Writers;

public class JJMasterDataOptionsWriter : BaseWriter
{
    public override void Write()
    {
        var schema = new JSchema
        {
            Title = "JJMasterData",
            Description = "JSON schema for JJMasterData ASP.NET Core's appsettings.json file"
        };

        var jjmasterdata = new KeyValuePair<string, JSchema>("JJMasterData", Generator.Generate(typeof(MasterDataCommonsOptions)));

        schema.Properties.Add(jjmasterdata);

        WriteSchema("JJMasterDataOptions", schema);
    }
}