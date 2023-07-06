using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity;
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
        string prop = "public @PropType @PropName { get; set; } ";

        var dicParser = await DataDictionaryRepository.GetMetadataAsync(elementName);
        var propsBuilder = new StringBuilder();

        foreach (var item in dicParser.Fields.ToList())
        {
            var nameProp = StringManager.GetStringWithoutAccents(item.Name.Replace(" ", "").Replace("-", " ").Replace("_", " "));
            var typeProp = GetPropertyType(item.DataType, item.IsRequired);
            var propField = prop.Replace("@PropName", ToCamelCase(nameProp)).Replace("@PropType", typeProp);

            propsBuilder.AppendLine($"\t[JsonProperty( \"{item.Name}\")] ");
            propsBuilder.AppendLine("\t"+propField);
            propsBuilder.AppendLine("");

        }

        var resultClass = new StringBuilder();

        resultClass.AppendLine($"public class {dicParser.Name}" + "\r\n{");
        resultClass.AppendLine(propsBuilder.ToString());
        resultClass.AppendLine("\r\n}");

        return resultClass.ToString();
    }

    private static string GetPropertyType(FieldType dataTypeField, bool required)
    {
        return dataTypeField switch
        {
            FieldType.Date or FieldType.DateTime or FieldType.DateTime2 => "DateTime",
            FieldType.Float => "double",
            FieldType.Int => "int",
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