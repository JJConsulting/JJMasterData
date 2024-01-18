using JJMasterData.Commons.Data.Entity.Models;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Models;


public class DataElementMapFilter
{
    [JsonProperty("fieldName")]
    public string FieldName { get; set; }

    [JsonProperty("expressionValue")]
    [AsyncExpression]
    public string ExpressionValue { get; set; }

}