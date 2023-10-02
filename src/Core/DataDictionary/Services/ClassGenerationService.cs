using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;

namespace JJMasterData.Core.DataDictionary.Services;

public class ClassGenerationService
{
    private IDataDictionaryRepository DataDictionaryRepository { get; }

    public ClassGenerationService(IDataDictionaryRepository dataDictionaryRepository)
    {
        DataDictionaryRepository = dataDictionaryRepository;
    }

    public async Task<string> GetClassSourceCode(string elementName)
    {
        const string propertyTemplate = "public @PropertyType @PropertyName { get; set; } ";

        var formElement = await DataDictionaryRepository.GetFormElementAsync(elementName);
        var properties = new StringBuilder();

        foreach (var item in formElement.Fields.ToList())
        {
            var propertyName = StringManager.GetStringWithoutAccents(item.Name.Replace(" ", "").Replace("-", " ").Replace("_", " "));
            var propertyType = GetPropertyType(item.DataType, item.IsRequired);
            var property = propertyTemplate.Replace("@PropertyName", ToCamelCase(propertyName)).Replace("@PropertyType", propertyType);

            properties.AppendLine($"\t[JsonProperty( \"{item.Name}\")] ");
            properties.AppendLine($"\t{property}");
            properties.AppendLine("");

        }

        var classResult = new StringBuilder();

        classResult.AppendLine($"public class {formElement.Name}\r\n{{");
        classResult.AppendLine(properties.ToString());
        classResult.AppendLine("\r\n}");

        return classResult.ToString();
    }

    private static string GetPropertyType(FieldType dataTypeField, bool required)
    {
        return dataTypeField switch
        {
            FieldType.Date or FieldType.DateTime or FieldType.DateTime2 => "DateTime",
            FieldType.Float => "double",
            FieldType.Int => "int",
            FieldType.Bit => "bool",
            FieldType.NText or FieldType.NVarchar or FieldType.Text or FieldType.Varchar => required ? "string" : "string?",
            _ => "",
        };
    }

    private static string ToCamelCase(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        
        string formattedValue = string.Empty;
        value.Split(' ').ToList().ForEach(x => formattedValue += x.FirstCharToUpper());

        return formattedValue;

    }
}