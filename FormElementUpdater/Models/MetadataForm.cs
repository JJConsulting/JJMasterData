using System.Runtime.Serialization;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.DataDictionary;
using Newtonsoft.Json;

namespace JJMasterData.FormElementUpdater.Models;

[DataContract]
public class MetadataForm
{
    [JsonProperty("formfields")]
    public List<MetadataFormField> FormFields { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("subtitle")]
    public string SubTitle { get; set; }

    [JsonProperty("panels")]
    public List<FormElementPanel> Panels { get; set; }

    public MetadataForm()
    {
        Panels = new List<FormElementPanel>();
        FormFields = new List<MetadataFormField>();
    }
}