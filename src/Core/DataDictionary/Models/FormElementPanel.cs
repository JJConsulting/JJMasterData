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
    [Display(Name = "Title")]
    [SyncExpression]
    public string? Title { get; set; }

    [JsonProperty("subtitle")]
    [Display(Name = "SubTitle")]
    [SyncExpression]
    public string? SubTitle { get; set; }

    [JsonProperty("layout")]
    [Display(Name = "Layout")]
    public PanelLayout Layout { get; set; } = PanelLayout.Well;

    [JsonProperty("color")]
    [Display(Name = "Color")]
    public BootstrapColor Color { get; set; } = BootstrapColor.Default;

    [JsonProperty("icon")]
    [Display(Name = "Icon")]
    public IconType? Icon { get; set; }
    
    [JsonProperty("expandedByDefault")]
    [Display(Name = "Expanded By Default")]
    public bool ExpandedByDefault { get; set; }

    /// <remarks>
    /// [See expressions](../articles/expressions.md)
    /// </remarks>
    [JsonProperty("visibleExpression")]
    [Display(Name = "Visible Expression")]
    [SyncExpression]
    public string VisibleExpression { get; set; } = "val:1";

    /// <remarks>
    /// [See expressions](../articles/expressions.md)
    /// </remarks>
    [JsonProperty("enableExpression")]
    [Display(Name = "Enable Expression")]
    [SyncExpression]
    public string EnableExpression { get; set; } = "val:1";

    [JsonProperty("cssClass")]
    [Display(Name = "CSS Class")]
    public string? CssClass { get; set; }

    public bool HasTitle()
    {
        return !string.IsNullOrEmpty(Title) | !string.IsNullOrEmpty(SubTitle);
    }

    public FormElementPanel DeepCopy()
    {
        return (FormElementPanel)MemberwiseClone();
    }
}