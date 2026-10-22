using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace JJConsulting.Html.Bootstrap.Abstractions;

public abstract class HtmlComponent : ComponentBase, IHtmlContent
{
    /// <summary>
    /// Returns the object representation of the HTML
    /// </summary>
    protected abstract HtmlBuilder BuildHtml();

    public HtmlBuilder GetHtmlBuilder()
    {
        return Visible ? BuildHtml() : new HtmlBuilder();
    }

    /// <summary>
    /// Renders the content in HTML.
    /// </summary>
    /// <returns>
    /// The HTML string.
    /// </returns>
    public string GetHtml()
    {
        return Visible ? BuildHtml().ToString() : string.Empty;
    }

    public void WriteTo(TextWriter writer, HtmlEncoder encoder)
    {
        GetHtmlBuilder().WriteTo(writer, encoder);
    }
}