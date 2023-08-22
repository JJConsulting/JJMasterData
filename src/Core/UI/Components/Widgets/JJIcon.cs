using System.Diagnostics.CodeAnalysis;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Html;

namespace JJMasterData.Core.Web.Components;

public class JJIcon : HtmlComponent
{

    /// <remarks>
    /// Font Awesome icon class
    /// </remarks>
    public required string IconClass { get; set; }
    public string? Color { get; set; }
    public string? Title { get; set; }

    public JJIcon() { }

    [SetsRequiredMembers]
    public JJIcon(IconType icon)
    {
        IconClass = icon.GetCssClass();
    }
    
    [SetsRequiredMembers]
    public JJIcon(IconType icon, string color) : this(icon)
    {
        Color = color;
    }
    
    [SetsRequiredMembers]
    public JJIcon(IconType icon, string color, string title) : this(icon, color)
    {
        Title = title;
    }
    
    [SetsRequiredMembers]
    public JJIcon(string iconClass)
    {
        IconClass = iconClass;
    }
    
    [SetsRequiredMembers]
    public JJIcon(string iconClass, string color) : this(iconClass)
    {
        Color = color;
    }
    
    [SetsRequiredMembers]
    public JJIcon(string iconClass, string color, string title) : this(iconClass, color)
    {
        Title = title;
    }
    
    internal override HtmlBuilder BuildHtml()
    {
        var element = new HtmlBuilder(HtmlTag.Span)
            .WithNameAndId(Name)
            .WithAttributes(Attributes)
            .WithCssClass(IconClass)
            .WithCssClass(CssClass)
            .WithToolTip(Title)
            .WithAttributeIf(!string.IsNullOrEmpty(Color), "style",$"color:{Color}");

        return element;
    }
}
