#nullable enable

using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary;


public class FormElementPanel
{
    [JsonProperty("id")]
    public int PanelId { get; set; }

    [JsonProperty("title")]
    public string? Title { get; set; }

    [JsonProperty("subtitle")]
    public string? SubTitle { get; set; }

    [JsonProperty("layout")]
    public PanelLayout Layout { get; set; }

    [JsonProperty("color")]
    public PanelColor Color { get; set; }

    [JsonProperty("expandedByDefault")]
    public bool ExpandedByDefault { get; set; }

    /// <remarks>
    /// [See expressions](../articles/expressions.md)
    /// </remarks>
    [JsonProperty("visibleExpression")]
    public string VisibleExpression { get; set; }

    /// <remarks>
    /// [See expressions](../articles/expressions.md)
    /// </remarks>
    [JsonProperty("enableExpression")]
    public string EnableExpression { get; set; }

    [JsonProperty("cssClass")]
    public string? CssClass { get; set; }

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