using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Html;

namespace JJMasterData.Core.Web.Components;

public class JJIcon : HtmlComponent
{

    /// <remarks>
    /// Font Awesome icon class
    /// </remarks>
    public string IconClass { get; set; }
    public string Color { get; set; }
    public string Title { get; set; }

    internal JJIcon() { }

    public JJIcon(IconType icon)
    {
        IconClass = icon.GetCssClass();
    }

    public JJIcon(IconType icon, string color) : this(icon)
    {
        Color = color;
    }

    public JJIcon(IconType icon, string color, string title) : this(icon, color)
    {
        Title = title;
    }

    public JJIcon(string iconClass)
    {
        IconClass = iconClass;
    }

    public JJIcon(string iconClass, string color) : this(iconClass)
    {
        Color = color;
    }

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
