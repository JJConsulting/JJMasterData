#nullable disable

using System.Collections;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;
using Newtonsoft.Json;

namespace JJMasterData.ConsoleApp.Models.FormElementMigration;


public class MetadataFormField
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("component")]
    public FormComponent Component { get; set; }

    [JsonProperty("visibleexpression")]
    public string VisibleExpression { get; set; }

    [JsonProperty("enableexpression")]
    public string EnableExpression { get; set; }

    [JsonProperty("triggerexpression")]
    public string TriggerExpression { get; set; }

    [JsonProperty("order")]
    public int Order { get; set; }

    [JsonProperty("linegroup")]
    public int LineGroup { get; set; }

    [JsonProperty("cssclass")]
    public string CssClass { get; set; }

    [JsonProperty("helpdescription")]
    public string HelpDescription { get; set; }

    [JsonProperty("dataitem")]
    public FormElementDataItem DataItem { get; set; }

    [JsonProperty("datafile")]
    public FormElementDataFile DataFile { get; set; }

    [JsonProperty("export")]
    public bool Export { get; set; }

    [JsonProperty("validaterequest")]
    public bool? ValidateRequest { get; set; }

    [JsonProperty("autopostback")]
    public bool AutoPostBack { get; set; }

    [JsonProperty("maxvalue")]
    public float? MaxValue { get; set; }

    [JsonProperty("minvalue")]
    public float? MinValue { get; set; }

    [JsonProperty("numberdecimalplaces")]
    public int NumberOfDecimalPlaces { get; set; }

    [JsonProperty("actions")]
    public FormElementFieldActionList Actions { get; set; }

    [JsonProperty("attributes")]
    public Hashtable Attributes { get; set; }

    [JsonProperty("panelid")]
    public int PanelId { get; set; }

    [JsonProperty("internalnotes")]
    public string InternalNotes { get; set; }
}