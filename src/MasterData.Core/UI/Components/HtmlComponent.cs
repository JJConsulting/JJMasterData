using System.Diagnostics;
using JJConsulting.Html;


namespace JJMasterData.Core.UI.Components;

public abstract class HtmlComponent : ComponentBase
{
    /// <summary>
    /// Returns the object representation of the HTML
    /// </summary>
    internal abstract HtmlBuilder BuildHtml();

    public HtmlBuilder GetHtmlBuilder()
    {
        return Visible ? BuildHtml() : null;
    }

    /// <summary>
    /// Renders the content in HTML.
    /// </summary>
    /// <returns>
    /// The HTML string.
    /// </returns>
    public string GetHtml()
    {
        if (Visible)
            return BuildHtml()?.ToString(indented:Debugger.IsAttached);
        
        return string.Empty;
    }

    public static explicit operator RenderedComponentResult(HtmlComponent component) => new(component.GetHtmlBuilder());
}