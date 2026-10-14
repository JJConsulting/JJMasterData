using JJConsulting.FontAwesome;
using JJConsulting.Html.Bootstrap.Abstractions;
using JJConsulting.Html.Bootstrap.Extensions;
using JJConsulting.Html.Bootstrap.Models;
using JJConsulting.Html.Extensions;

namespace JJConsulting.Html.Bootstrap.Components;

public class JJIcon : HtmlComponent
{
    public string? IconClass { get; set; }
    public string? Color { get; set; }
    public string? Tooltip { get; set; }

    public JJIcon()
    {
    }

    public JJIcon(FontAwesomeIcon icon)
    {
        IconClass = icon.CssClass;
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

    protected override HtmlBuilder BuildHtml()
    {
        var span = new HtmlBuilder(HtmlTag.Span)
            .WithNameAndId(Name)
            .WithAttributes(Attributes)
            .WithCssClass($"{IconClass} {CssClass}")
            .WithToolTip(Tooltip)
            .WithAttributeIf(!string.IsNullOrEmpty(Color), "style", $"color:{Color}");

        return span;
    }
}