using JJMasterData.Web.Configuration.Options;
using Newtonsoft.Json.Schema;

namespace JJMasterData.SchemaGenerator.Writers;

public class MasterDataOptionsWriter : BaseWriter
{
    public virtual void Write()
    {
        var schema = new JSchema
        {
            Title = "JJMasterData",
            Description = "JSON schema for JJMasterData .NET appsettings.json file"
        };

        var jjmasterdata = new KeyValuePair<string, JSchema>("JJMasterData", Generator.Generate(typeof(MasterDataWebOptions)));

        schema.Properties.Add(jjmasterdata);

        WriteSchema("jjmasterdata", schema);
    }
}