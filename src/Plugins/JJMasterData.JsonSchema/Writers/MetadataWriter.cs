using JJMasterData.Core.DataDictionary;

namespace JJMasterData.JsonSchema.Writers;

public class MetadataWriter : BaseWriter
{
    public override async Task WriteAsync()
    {
        var schema = Generator.Generate(typeof(Metadata));
        await WriteSchemaAsync("Metadata", schema);
    }
}