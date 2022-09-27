using System.Text;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Html;

namespace JJMasterData.Core.WebComponents;

public class JJIcon : JJBaseView
{
    /// <summary>
    /// Classe do icone
    /// </summary>
    /// <remarks>
    /// classe da fonte fontawesome ou glyphicons 
    /// </remarks>
    public string IconClass { get; set; }

    public string Color { get; set; }
    public string Title { get; set; }

    public JJIcon() { }

    public JJIcon(IconType icon)
    {
        IconClass = IconHelper.GetClassName(icon);
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
    
    internal override HtmlElement GetHtmlElement()
    {
        var element = new HtmlElement(HtmlTag.Span)
            .WithNameAndId(Name)
            .WithCssClass(IconClass)
            .WithCssClass(CssClass)
            .WithToolTip(Translate.Key(Title))
            .WithAttributeIf(!string.IsNullOrEmpty(Color), "style",$"color:{Color}");

        return element;
    }
}
