using System;

namespace JJMasterData.Core.Html;

/// <inheritdoc cref="IHtmlBuilder"/>
public class HtmlBuilder : IHtmlBuilder
{
    private HtmlElement _element;

    /// <inheritdoc/>
    public HtmlElement StartElement(HtmlTag tag)
    {
        _element = new HtmlElement(tag);

        return _element;
    }

    /// <inheritdoc/>
    public string RenderHtml()
    {
        if (_element == null)
        {
            throw new ArgumentNullException("HTML Element", "HTML element is not build. Use StartElement method to build your HTML element");
        }

        return _element.GetElementHtml();
    }


    /// <inheritdoc/>
    public void Reset()
    {
        _element = null;
    }
}
