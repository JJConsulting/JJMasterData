using JJMasterData.Core.DataDictionary.Models;
using Newtonsoft.Json;

namespace JJMasterData.WebApi.Models;

[JsonObject("formValues")]
public class FormValues
{
    [JsonProperty("value")]
    public object? Value { get; set; }

    [JsonProperty("enable")]
    public bool Enable { get; set; }

    [JsonProperty("visible")]
    public bool Visible { get; set; }

    [JsonProperty("dataItems")]
    public IList<DataItemValue>? DataItems { get; set; }
}