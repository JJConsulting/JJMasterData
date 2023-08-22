using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary;


public class DataElementMapFilter
{
    [JsonProperty("fieldName")] public string FieldName { get; set; } = null!;

    [JsonProperty("expressionValue")]
    public string ExpressionValue { get; set; } = null!;

}