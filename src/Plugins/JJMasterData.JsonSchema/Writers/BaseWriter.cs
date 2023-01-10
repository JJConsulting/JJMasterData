using JJMasterData.JsonSchema.Models;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using Newtonsoft.Json.Serialization;

namespace JJMasterData.JsonSchema.Writers;

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

    public abstract Task WriteAsync();
    
    private static string GetFilePath(string? fileName)
    {
        string workingDirectory = Environment.CurrentDirectory;
        string? projectDirectory = Directory.GetParent(workingDirectory)?.Parent?.Parent?.FullName;

        string path = Path.Combine(projectDirectory ?? Environment.CurrentDirectory, $"{fileName}.json");

        return path;
    }

    protected static async Task WriteSchemaAsync(string fileName, JSchema schema)
    {
        await File.WriteAllTextAsync(GetFilePath(fileName), schema.ToString());
    }
}