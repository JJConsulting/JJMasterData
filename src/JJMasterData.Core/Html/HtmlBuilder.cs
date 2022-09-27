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
    /// Start fluent building HTML element.
    /// </summary>
    public HtmlElement StartElement(HtmlElement element)
    {
        _element = element ?? throw new ArgumentNullException(nameof(element));
        return _element;
    }
    
    /// <summary>
    /// Returns the root HTML element.
    /// </summary>
    public HtmlElement GetElement() => _element;

    /// <summary>
    /// Render HTML content based on built element.
    /// </summary>
    public string RenderHtml(bool indentHtml = true)
    {
        if (_element == null)
        {
            throw new ArgumentNullException("HTML Element", "HTML element is not build. Use StartElement method to build your HTML element");
        }
        int tabCount = indentHtml ? 1 : 0;
        return _element.GetElementHtml(tabCount);
    }

    /// <summary>
    /// Reset builder element.
    /// </summary>
    public void Reset()
    {
        _element = null;
    }
}
