using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.UI.Components.Controls;

public abstract class HtmlControl : ControlBase
{
    protected HtmlControl(IHttpContext currentContext) : base(currentContext)
    {
    }
    
    /// <summary>
    /// Returns the object representation of the HTML
    /// </summary>
    internal abstract HtmlBuilder BuildHtml();

    public HtmlBuilder? GetHtmlBuilder()
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
        return Visible ? BuildHtml().ToString(true) : string.Empty;
    }
}