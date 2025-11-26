#nullable enable
using System.Collections.Generic;
using System.Web;
using JJConsulting.Html;
using JJMasterData.Core.UI;
using JJMasterData.Core.UI.Components;

namespace JJMasterData.Core.Html;

public static class HtmlBuilderExtensions
{
    public static HtmlBuilder PrependComponent(this HtmlBuilder htmlBuilder, HtmlComponent? component)
    {
        if (component != null)
            htmlBuilder.Prepend(component.GetHtmlBuilder());

        return htmlBuilder;
    }
    
    /// <summary>
    /// Insert a <see cref="HtmlComponent"/> as a child of caller builder.
    /// </summary>
    public static HtmlBuilder AppendComponent(this HtmlBuilder htmlBuilder, HtmlComponent? component)
    {
        if (component is not null)
            htmlBuilder.Append(component.GetHtmlBuilder());

        return htmlBuilder;
    }
    
    
    /// <summary>
    /// Set a custom Bootstrap data attribute to HTML builder.
    /// </summary>
    public static HtmlBuilder WithDataAttribute(this HtmlBuilder htmlBuilder, string name, string value)
    {
        var attributeName = BootstrapHelper.Version >= 5 ? $"data-bs-{name}" : $"data-{name}";
        return htmlBuilder.WithAttribute(attributeName, value);
    }
    
    public static HtmlBuilder WithAttribute(
        this HtmlBuilder htmlBuilder,
        string attributeName)
    {
        htmlBuilder.WithAttribute(attributeName, attributeName);
        return htmlBuilder;
    }
    
    /// <summary>
    /// Sets a tooltip to the HTML Tag
    /// </summary>
    public static HtmlBuilder WithToolTip(this HtmlBuilder htmlBuilder, string? tooltip)
    {
        if (tooltip == null || string.IsNullOrEmpty(tooltip)) 
            return htmlBuilder;
        
        htmlBuilder.WithAttribute("title",tooltip);
        htmlBuilder.WithAttribute(BootstrapHelper.DataToggle, "tooltip");

        return htmlBuilder;
    }
}