using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.SchemaGenerator.Writers;

public class AssemblyClassWriter : BaseWriter
{
    public async Task WriteAsync(string? className)
    {
        //Used to load JJMasterData assemblies
#pragma warning disable CS0168
        FormElement formElement;
        Element element;
#pragma warning restore CS0168
        
        var classType = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
            from type in assembly.GetTypes()
            where type.Name == className
            select type).FirstOrDefault();
        
        if(classType == null)
            Console.WriteLine("Invalid class name.\n");
        
        else
        {
            var schema = Generator.Generate(classType);
            string path = GetFilePath(className!.ToLower());
            
            await File.WriteAllTextAsync(path, schema.ToString());

            Console.WriteLine("File successfuly generated!\n");
        }

    }
}