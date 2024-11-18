using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JJMasterData.Commons.Data.Entity.Models;


namespace JJMasterData.Core.DataDictionary.Models;


public class DataElementMapFilter
{
    [JsonPropertyName("fieldName")]
    public string FieldName { get; set; }

    [JsonPropertyName("expressionValue")]
    [SyncExpression]
    [Display(Name = "Expression")]
    public string ExpressionValue { get; set; }

    public DataElementMapFilter DeepCopy()
    {
        return (DataElementMapFilter)MemberwiseClone();
    }

}