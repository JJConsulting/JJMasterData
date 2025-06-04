#nullable disable

using System.Runtime.Serialization;
using JJMasterData.Core.DataDictionary.Models;
using Newtonsoft.Json;

namespace JJMasterData.LegacyMetadataMigrator.FormElementMigration;

[DataContract]
public class MetadataForm
{
    [JsonProperty("formfields")]
    public List<MetadataFormField> FormFields { get; set; } = [];

    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("subtitle")]
    public string SubTitle { get; set; }

    [JsonProperty("panels")]
    public List<FormElementPanel> Panels { get; set; } = [];
}