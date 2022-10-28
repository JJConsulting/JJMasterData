using System;
using System.Collections.Generic;
using System.Text;

namespace JJMasterData.Core.Html;

/// <summary>
/// Implementation of HTML builder.
/// </summary>
public partial class HtmlBuilder
{
    private readonly Dictionary<string, string> _attributes;
    private readonly string _rawText;
    private readonly bool _hasRawText;
    private readonly HtmlBuilderCollection _children;

    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlBuilder"/> class.
    /// </summary>
    internal HtmlBuilder()
    {
        _attributes = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        _children = new HtmlBuilderCollection();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlBuilder"/> class.
    /// </summary>
    /// <param name="rawText"></param>
    internal HtmlBuilder(string rawText) : this()
    {
        _rawText = rawText;
        _hasRawText = true;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlBuilder"/> class.
    /// </summary>
    internal HtmlBuilder(HtmlTag tag) : this()
    {
        Tag = new HtmlElementTag(tag);
    }

    /// <summary>
    /// Tag of the current builder.
    /// </summary>
    public HtmlElementTag Tag { get; private set; }

    /// <summary>
    /// Gets current builder HTML.
    /// </summary>
    internal string GetHtml(int tabCount = 0)
    {
        var html = new StringBuilder();
        if (tabCount > 0)
            html.AppendLine("").Append('\t', tabCount);

        if (_hasRawText || Tag == null)
        {
            html.Append(GetHtmlContent(tabCount));
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
        html.Append(GetHtmlContent(tabCount));

        if (tabCount > 0)
            html.AppendLine("").Append('\t', tabCount);

        html.Append("</");
        html.Append(Tag.TagName.ToString().ToLower());
        html.Append(">");

        return html.ToString();
    }

    private string GetHtmlContent(int tabCount)
    {
        if (tabCount > 0)
            tabCount++;

        var content = new StringBuilder();
        foreach (var child in _children)
        {
            content.Append(child.GetHtml(tabCount));
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
