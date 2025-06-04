

using System.Text.Json.Serialization;

namespace JJMasterData.Core.DataDictionary.Models;

public class FormActionRelationField
{
    [JsonPropertyName("internalField")] public string InternalField { get; set; }

    [JsonPropertyName("redirectField")] public string RedirectField { get; set; }

    public FormActionRelationField DeepCopy() => (FormActionRelationField)MemberwiseClone();
}