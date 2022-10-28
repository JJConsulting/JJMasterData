using System;
using System.Collections.Generic;

namespace JJMasterData.Core.Html;

/// <summary>
/// Collection of <see cref="HtmlBuilder"/>.
/// </summary>
public class HtmlBuilderCollection : List<HtmlBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlBuilderCollection"/> class.
    /// </summary>
    internal HtmlBuilderCollection()
    {
    }

    /// <summary>
    /// Insert HTML builder into current collection.
    /// </summary>
    /// <param name="elementAction"></param>
    /// <returns></returns>
    public HtmlBuilderCollection Append(Action<HtmlBuilder> elementAction)
    {
        var element = new HtmlBuilder();
        elementAction.Invoke(element);
        this.Add(element);
        return this;
    }
}
