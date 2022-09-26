

namespace JJMasterData.Core.Html;

/// <summary>
/// Main HTML builder.
/// </summary>
public interface IHtmlBuilder
{
    /// <summary>
    /// Start fluent building HTML element.
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    HtmlElement StartElement(HtmlTag tag);

    /// <summary>
    /// Render HTML content based on built element.
    /// </summary>
    /// <returns></returns>
    string RenderHtml();

    /// <summary>
    /// Reset builder element.
    /// </summary>
    void Reset();

    
}
