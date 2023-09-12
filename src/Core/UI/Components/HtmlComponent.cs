using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Html;

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
        return Visible ? BuildHtml()?.ToString(true) : string.Empty;
    }

    public static explicit operator RenderedComponentResult(HtmlComponent component) => new(component.GetHtmlBuilder());
}