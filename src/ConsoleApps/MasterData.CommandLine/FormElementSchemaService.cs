using System.Text.Json;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;
using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.CommandLine;

public static class FormElementSchemaService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        WriteIndented = true
    };

    public static async Task WriteAsync(string outputPath, CancellationToken cancellationToken)
    {
        var schema = JsonOptions.GetJsonSchemaAsNode(typeof(FormElement),
            new JsonSchemaExporterOptions { TreatNullObliviousAsNonNullable = true });
        schema["$schema"] = "https://json-schema.org/draft/2020-12/schema";
        var fullPath = Path.GetFullPath(outputPath);
        var directory = Path.GetDirectoryName(fullPath)!;
        Directory.CreateDirectory(directory);

        await File.WriteAllTextAsync(fullPath, schema.ToJsonString(JsonOptions) + Environment.NewLine,
            cancellationToken);
    }
}
