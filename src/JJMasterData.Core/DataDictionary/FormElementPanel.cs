using System;
using System.Runtime.Serialization;

namespace JJMasterData.Core.DataDictionary;

[Serializable]
[DataContract]
public class FormElementPanel
{
    [DataMember(Name = "id")]
    public int PanelId { get; set; }

    [DataMember(Name = "title")]
    public string Title { get; set; }

    [DataMember(Name = "subtitle")]
    public string SubTitle { get; set; }

    [DataMember(Name = "layout")]
    public PanelLayout Layout { get; set; }

    [DataMember(Name = "color")]
    public PanelColor Color { get; set; }

    [DataMember(Name = "expandedByDefault")]
    public bool ExpandedByDefault { get; set; }

    [DataMember(Name = "visibleExpression")]
    public string VisibleExpression { get; set; }

    [DataMember(Name = "enableExpression")]
    public string EnableExpression { get; set; }

    [DataMember(Name = "cssClass")]
    public string CssClass { get; set; }

    public FormElementPanel()
    {
        Layout = PanelLayout.Well;
        Color = PanelColor.Default;
        VisibleExpression = "val:1";
        EnableExpression = "val:1";
    }

    public bool HasTitle()
    {
        return !string.IsNullOrEmpty(Title) | !string.IsNullOrEmpty(SubTitle);
    }

}