using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Models;

public class FormActionRelationField
{
    [JsonProperty("internalField")]
    public string InternalField { get; set; }

    [JsonProperty("redirectField")]
    public string RedirectField { get; set; }
}