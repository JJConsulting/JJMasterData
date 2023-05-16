#nullable disable

using System.Collections;
using System.Runtime.Serialization;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.FormElementUpdater.Models;

[DataContract]
public class MetadataFormField
{
    [DataMember(Name = "name")]
    public string Name { get; set; }

    [DataMember(Name = "component")]
    public FormComponent Component { get; set; }

    [DataMember(Name = "visibleexpression")]
    public string VisibleExpression { get; set; }

    [DataMember(Name = "enableexpression")]
    public string EnableExpression { get; set; }

    [DataMember(Name = "triggerexpression")]
    public string TriggerExpression { get; set; }

    [DataMember(Name = "order")]
    public int Order { get; set; }

    [DataMember(Name = "linegroup")]
    public int LineGroup { get; set; }

    [DataMember(Name = "cssclass")]
    public string CssClass { get; set; }

    [DataMember(Name = "helpdescription")]
    public string HelpDescription { get; set; }

    [DataMember(Name = "dataitem")]
    public FormElementDataItem DataItem { get; set; }

    [DataMember(Name = "datafile")]
    public FormElementDataFile DataFile { get; set; }

    [DataMember(Name = "export")]
    public bool Export { get; set; }

    [DataMember(Name = "validaterequest")]
    public bool? ValidateRequest { get; set; }

    [DataMember(Name = "autopostback")]
    public bool AutoPostBack { get; set; }

    [DataMember(Name = "maxvalue")]
    public float? MaxValue { get; set; }

    [DataMember(Name = "minvalue")]
    public float? MinValue { get; set; }

    [DataMember(Name = "numberdecimalplaces")]
    public int NumberOfDecimalPlaces { get; set; }

    [DataMember(Name = "actions")]
    public FormElementFieldActions Actions { get; set; }

    [DataMember(Name = "attributes")]
    public Hashtable Attributes { get; set; }

    [DataMember(Name = "panelid")]
    public int PanelId { get; set; }

    [DataMember(Name = "internalnotes")]
    public string InternalNotes { get; set; }
}