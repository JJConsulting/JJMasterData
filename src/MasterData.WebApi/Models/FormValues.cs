using System.Text.Json.Serialization;
using JJMasterData.Core.DataDictionary.Models;


namespace JJMasterData.WebApi.Models;

public class FormValues
{
    [JsonPropertyName("value")]
    public object? Value { get; set; }

    [JsonPropertyName("enable")]
    public bool Enable { get; set; }

    [JsonPropertyName("visible")]
    public bool Visible { get; set; }

    [JsonPropertyName("dataItems")]
    public IList<DataItemValue>? DataItems { get; set; }
}