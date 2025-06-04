#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JJMasterData.Commons.Data.Entity.Models;


namespace JJMasterData.Core.DataDictionary.Models;

public class FormElementPanel
{
    [JsonPropertyName("id")]
    public int PanelId { get; set; }

    [JsonPropertyName("title")]
    [Display(Name = "Title")]
    public string? Title { get; set; }

    [JsonPropertyName("subtitle")]
    [Display(Name = "SubTitle")]
    public string? SubTitle { get; set; }

    [JsonPropertyName("layout")]
    [Display(Name = "Layout")]
    public PanelLayout Layout { get; set; } = PanelLayout.Well;

    [JsonPropertyName("color")]
    [Display(Name = "Color")]
    public BootstrapColor Color { get; set; } = BootstrapColor.Default;

    [JsonPropertyName("icon")]
    [Display(Name = "Icon")]
    public IconType? Icon { get; set; }
    
    [JsonPropertyName("expandedByDefault")]
    [Display(Name = "Expanded By Default")]
    public bool ExpandedByDefault { get; set; }

    /// <remarks>
    /// [See expressions](../articles/expressions.md)
    /// </remarks>
    [JsonPropertyName("visibleExpression")]
    [Display(Name = "Visible Expression")]
    public string VisibleExpression { get; set; } = "val:1";

    /// <remarks>
    /// [See expressions](../articles/expressions.md)
    /// </remarks>
    [JsonPropertyName("enableExpression")]
    [Display(Name = "Enable Expression")]
    public string EnableExpression { get; set; } = "val:1";

    [JsonPropertyName("cssClass")]
    [Display(Name = "CSS Class")]
    public string? CssClass { get; set; }

    public bool HasTitle()
    {
        return !string.IsNullOrEmpty(Title) || !string.IsNullOrEmpty(SubTitle);
    }

    public FormElementPanel DeepCopy()
    {
        return (FormElementPanel)MemberwiseClone();
    }
}