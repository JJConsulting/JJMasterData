#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JJMasterData.Core.UI.Components;

namespace JJMasterData.Core.Web.Html;

/// <summary>
/// Represents a mutable string of HTML tags.
/// </summary>
/// <example>
/// [!include[Test](../../../doc/JJMasterData.Documentation/articles/usages/htmlbuilder.md)]
/// </example>
public partial class HtmlBuilder
{
    private readonly string? _rawText;
    private readonly bool _hasRawText;
    private readonly IDictionary<string, string> _attributes;
    private readonly IList<HtmlBuilder> _children;

    /// <summary>
    /// Tag of the current builder.
    /// </summary>
    public HtmlBuilderTag? Tag { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlBuilder"/> class.
    /// </summary>
    public HtmlBuilder()
    {
        _attributes = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        _children = new List<HtmlBuilder>();
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
        Tag = new HtmlBuilderTag(tag);
    }

    /// <summary>
    /// Renders the instance to a String
    /// </summary>
    public override string ToString()
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

        if (Tag.TagName == HtmlTag.TextArea)
        {
            html.Append(GetHtmlContent(0));
        }
        else
        {
            html.Append(GetHtmlContent(tabCount));
            
            if (tabCount > 0)
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
        var attributes = new StringBuilder();
        foreach (var item in _attributes)
        {
            attributes.Append($" {item.Key}=\"{item.Value}\"");
        }
        return attributes.ToString();
    }
    public string GetAttribute(string key)
    {
        return _attributes[key];
    }
    public IEnumerable<HtmlBuilder> GetChildren()
    {
        return _children.ToList();
    } 
    public IEnumerable<HtmlBuilder> GetChildren(HtmlTag tagName)
    {
        return _children.Where(x => x.Tag?.TagName == tagName);
    }
    public string? GetRawText()
    {
        return _rawText;
    }
}
