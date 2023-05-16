using System.Runtime.Serialization;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.FormElementUpdater.Models;

[DataContract]
public class MetadataForm
{
    [DataMember(Name = "formfields")]
    public List<MetadataFormField> FormFields { get; set; }

    [DataMember(Name = "title")]
    public string Title { get; set; }

    [DataMember(Name = "subtitle")]
    public string SubTitle { get; set; }

    [DataMember(Name = "panels")]
    public List<FormElementPanel> Panels { get; set; }

    public MetadataForm()
    {
        Panels = new List<FormElementPanel>();
        FormFields = new List<MetadataFormField>();
    }
}