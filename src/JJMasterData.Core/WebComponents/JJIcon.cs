using System.Text;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;

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
    
    protected override string RenderHtml()
    {
        var sHtml = new StringBuilder();
        if (CssClass == null)
            CssClass = string.Empty;

        sHtml.Append($"<span class=\"{IconClass} {CssClass}\"");

        if (!string.IsNullOrEmpty(Color))
        {
            sHtml.Append(" style=\"color:");
            sHtml.Append(Color);
            sHtml.Append(";\"");
        }

        if (!string.IsNullOrEmpty(Title))
        {
            sHtml.Append($" {BootstrapHelper.DataToggle}=\"tooltip\" title=\"");
            sHtml.Append(Translate.Key(Title));
            sHtml.Append("\"");
        }
        sHtml.Append(">");
        sHtml.Append("</span>");
       
        return sHtml.ToString();
    }
}
