using System;
using System.Collections.Generic;

namespace JJMasterData.Core.Html;

/// <summary>
/// Collection of <see cref="HtmlElement"/>.
/// </summary>
public class HtmlElementsCollection : List<HtmlElement>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlElementsCollection"/> class.
    /// </summary>
    internal HtmlElementsCollection()
    {
    }

    /// <summary>
    /// Insert HTML element into current collection.
    /// </summary>
    /// <param name="elementAction"></param>
    /// <returns></returns>
    public HtmlElementsCollection Append(Action<HtmlElement> elementAction)
    {
        var element = new HtmlElement();
        elementAction.Invoke(element);
        this.Add(element);
        return this;
    }
}
