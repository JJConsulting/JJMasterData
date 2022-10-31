using System;
using System.Collections.Generic;
using System.Text;

namespace JJMasterData.Core.Html;

/// <summary>
/// Represents a mutable string of HTML tags.
/// </summary>
public partial class HtmlBuilder
{
    private readonly string _rawText;
    private readonly bool _hasRawText;
    private readonly Dictionary<string, string> _attributes;
    private readonly HtmlBuilderCollection _children;

    /// <summary>
    /// Tag of the current builder.
    /// </summary>
    public HtmlElementTag Tag { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlBuilder"/> class.
    /// </summary>
    public HtmlBuilder()
    {
        _attributes = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        _children = new HtmlBuilderCollection();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlBuilder"/> class.
    /// </summary>
    /// <param name="rawText"></param>
    public HtmlBuilder(string rawText) : this()
    {
        _rawText = rawText;
        _hasRawText = true;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlBuilder"/> class.
    /// </summary>
    public HtmlBuilder(HtmlTag tag) : this()
    {
        Tag = new HtmlElementTag(tag);
    }

    /// <summary>
    /// Gets current builder HTML.
    /// </summary>
    public new string ToString()
    {
        return GetHtml(0);
    }

    /// <summary>
    /// Gets current builder HTML.
    /// </summary>
    /// <param name="indentHtml">Generate html with indentation?</param>
    public string ToString(bool indentHtml)
    {
        int tabCount = indentHtml ? 1 : 0;
        return GetHtml(tabCount);
    }

    private string GetHtml(int tabCount)
    {
        var html = new StringBuilder();

        if (tabCount > 0)
        {
            html.AppendLine().Append(' ', tabCount * 2);
        }

        if (_hasRawText || Tag == null)
        {
            html.Append(_rawText);
            html.Append(GetHtmlContent(tabCount));

            return html.ToString();
        }

        html.Append('<');
        html.Append(Tag.TagName.ToString().ToLower());
        html.Append(GetAttributesHtml());

        if (!Tag.HasClosingTag)
        {
            html.Append(" />");
            return html.ToString();
        }

        html.Append('>');
        html.Append(GetHtmlContent(tabCount));

        if (tabCount > 0)
        {
            html.AppendLine().Append(' ', tabCount * 2);
        }

        html.Append("</");
        html.Append(Tag.TagName.ToString().ToLower());
        html.Append('>');

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
