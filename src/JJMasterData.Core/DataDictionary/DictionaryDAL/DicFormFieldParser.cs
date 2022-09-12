using System.Collections;
using System.Runtime.Serialization;

namespace JJMasterData.Core.DataDictionary.DictionaryDAL;

[DataContract]
public class DicFormFieldParser
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

    public DicFormFieldParser() { }

    public DicFormFieldParser(FormElementField f)
    {
        Name = f.Name;
        Component = f.Component;
        VisibleExpression = f.VisibleExpression;
        EnableExpression = f.EnableExpression;
        TriggerExpression = f.TriggerExpression;
        Order = f.Order;
        LineGroup = f.LineGroup;
        CssClass = f.CssClass;
        HelpDescription = f.HelpDescription;
        DataItem = f.DataItem;
        DataFile = f.DataFile;
        Export = f.Export;
        MinValue = f.MinValue;
        MaxValue = f.MaxValue;
        ValidateRequest = f.ValidateRequest;
        AutoPostBack = f.AutoPostBack;
        NumberOfDecimalPlaces = f.NumberOfDecimalPlaces;
        Actions = f.Actions;
        Attributes = f.Attributes;
        PanelId = f.PanelId;
        InternalNotes = f.InternalNotes;
    }
}