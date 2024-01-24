using System.ComponentModel.DataAnnotations;
using JJMasterData.Commons.Data.Entity.Models;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Models;


public class DataElementMapFilter
{
    [JsonProperty("fieldName")]
    public string FieldName { get; set; }

    [JsonProperty("expressionValue")]
    [AsyncExpression]
    [Display(Name = "Expression")]
    public string ExpressionValue { get; set; }

}