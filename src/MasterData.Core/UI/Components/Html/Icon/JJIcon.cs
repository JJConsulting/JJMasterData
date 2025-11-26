using JJConsulting.FontAwesome;
using JJConsulting.Html;
using JJConsulting.Html.Extensions;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.Html;


namespace JJMasterData.Core.UI.Components;

public class JJIcon : HtmlComponent
{

    /// <remarks>
    /// Font Awesome icon class
    /// </remarks>
    public string IconClass { get; set; }
    public string Color { get; set; }
    public string Tooltip { get; set; }
    
    public JJIcon()
    {
    }
    
    public JJIcon(FontAwesomeIcon icon)
    {
        IconClass = icon.GetCssClass();
    }

    public JJIcon(FontAwesomeIcon icon, string color) : this(icon)
    {
        Color = color;
    }

    public JJIcon(FontAwesomeIcon icon, string color, string tooltip) : this(icon, color)
    {
        Tooltip = tooltip;
    }

    public JJIcon(string iconClass)
    {
        IconClass = iconClass;
    }

    public JJIcon(string iconClass, string color) : this(iconClass)
    {
        Color = color;
    }

    public JJIcon(string iconClass, string color, string tooltip) : this(iconClass, color)
    {
        Tooltip = tooltip;
    }

    internal override HtmlBuilder BuildHtml()
    {
        var span = new HtmlBuilder(HtmlTag.Span)
            .WithNameAndId(Name)
            .WithAttributes(Attributes)
            .WithCssClass($"{IconClass} {CssClass}")
            .WithToolTip(Tooltip)
            .WithAttributeIf(!string.IsNullOrEmpty(Color), "style",$"color:{Color}");

        return span;
    }
}
