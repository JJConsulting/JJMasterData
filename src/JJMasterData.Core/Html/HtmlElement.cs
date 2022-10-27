using System;
using System.Collections.Generic;
using System.Text;

namespace JJMasterData.Core.Html;

/// <summary>
/// Implementation of HTML element.
/// </summary>
public partial class HtmlElement
{
    private readonly Dictionary<string, string> _attributes;
    private readonly string _rawText;
    private readonly bool _hasRawText;
    private readonly HtmlElementsCollection _children;

    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlElement"/> class.
    /// </summary>
    internal HtmlElement()
    {
        _attributes = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        _children = new HtmlElementsCollection();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlElement"/> class.
    /// </summary>
    /// <param name="rawText"></param>
    internal HtmlElement(string rawText) : this()
    {
        _rawText = rawText;
        _hasRawText = true;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlElement"/> class.
    /// </summary>
    internal HtmlElement(HtmlTag tag) : this()
    {
        Tag = new HtmlElementTag(tag);
    }

    /// <summary>
    /// Tag of the current element.
    /// </summary>
    public HtmlElementTag Tag { get; private set; }

    /// <summary>
    /// Gets current element HTML.
    /// </summary>
    internal string GetElementHtml(int tabCount = 0)
    {
        var html = new StringBuilder();
        if (tabCount > 0)
            html.AppendLine("").Append('\t', tabCount);

        if (_hasRawText || Tag == null)
        {
            html.Append(GetElementContent(tabCount));
            html.Append(_rawText);
            
            return html.ToString();
        }

        html.Append("<");
        html.Append(Tag.TagName.ToString().ToLower());
        html.Append(GetAttributesHtml());

        if (!Tag.HasClosingTag)
        {
            html.Append(" />");
            return html.ToString();
        }

        html.Append(">");
        html.Append(GetElementContent(tabCount));

        if (tabCount > 0)
            html.AppendLine("").Append('\t', tabCount);

        html.Append("</");
        html.Append(Tag.TagName.ToString().ToLower());
        html.Append(">");

        return html.ToString();
    }

    private string GetElementContent(int tabCount)
    {
        if (tabCount > 0)
            tabCount++;

        var content = new StringBuilder();
        foreach (var child in _children)
        {
            content.Append(child.GetElementHtml(tabCount));
        }

        return content.ToString();
    }
    
    private string GetAttributesHtml()
    {
        var attrs = new StringBuilder();
        foreach (var item in _attributes)
        {
            attrs.Append($" {item.Key}=\"{item.Value}\"");
        }

        return attrs.ToString();
    }
}
