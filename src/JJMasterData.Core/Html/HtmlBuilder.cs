using System;

namespace JJMasterData.Core.Html;

/// <summary>
/// Main HTML builder.
/// </summary>
public class HtmlBuilder 
{
    private HtmlElement _element;

    /// <summary>
    /// Start fluent building HTML element.
    /// </summary>
    public HtmlElement StartElement(HtmlTag tag)
    {
        _element = new HtmlElement(tag);

        return _element;
    }

    /// <summary>
    /// Render HTML content based on built element.
    /// </summary>
    public string RenderHtml()
    {
        if (_element == null)
        {
            throw new ArgumentNullException("HTML Element", "HTML element is not build. Use StartElement method to build your HTML element");
        }

        return _element.GetElementHtml();
    }


    /// <summary>
    /// Reset builder element.
    /// </summary>
    public void Reset()
    {
        _element = null;
    }
}
