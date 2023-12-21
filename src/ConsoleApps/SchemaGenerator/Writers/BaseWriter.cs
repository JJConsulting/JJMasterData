using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using Newtonsoft.Json.Serialization;

namespace JJMasterData.SchemaGenerator.Writers;

public abstract class BaseWriter 
{
    protected readonly JSchemaGenerator Generator;

    protected BaseWriter()
    {
        Generator = new JSchemaGenerator
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new PascalCaseNamingStrategy(true,true)
            }
        };
        
        Generator.GenerationProviders.Add(new StringEnumGenerationProvider());
    }

    private static string GetFilePath(string? fileName)
    {
        string workingDirectory = Environment.CurrentDirectory;
        string? projectDirectory = Directory.GetParent(workingDirectory)?.Parent?.Parent?.FullName;

        string path = Path.Combine(projectDirectory ?? Environment.CurrentDirectory, $"{fileName}.json");

        return path;
    }

    protected static void WriteSchema(string fileName, JSchema schema)
    {
        File.WriteAllText(GetFilePath(fileName), schema.ToString());
    }
}