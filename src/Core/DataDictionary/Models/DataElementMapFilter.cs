using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Models;


public class DataElementMapFilter
{
    [JsonProperty("fieldName")]
    public string FieldName { get; set; }

    [JsonProperty("expressionValue")]
    public string ExpressionValue { get; set; }

}