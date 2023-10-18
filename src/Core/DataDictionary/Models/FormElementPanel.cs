#nullable enable

using System.ComponentModel.DataAnnotations;
using JJMasterData.Commons.Data.Entity.Models;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Models;


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
    [Display(Name = "Visible Expression")]
    [BooleanExpression]
    public string VisibleExpression { get; set; }

    /// <remarks>
    /// [See expressions](../articles/expressions.md)
    /// </remarks>
    [JsonProperty("enableExpression")]
    [Display(Name = "Enable Expression")]
    [BooleanExpression]
    public string EnableExpression { get; set; }

    [JsonProperty("cssClass")]
    [Display(Name = "Css Class")]
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