﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Text;


namespace JJMasterData.Core.UI.Html;

/// <summary>
/// Represents a mutable string of HTML tags.
/// </summary>
/// <example>
/// [!include[Example](../../../doc/Documentation/articles/usages/htmlbuilder.md)]
/// </example>
public partial class HtmlBuilder
{
    private readonly string? _rawText;
    private readonly bool _hasRawText;
    private readonly Dictionary<string, string> _attributes;
    private readonly List<HtmlBuilder> _children;

    /// <summary>
    /// Tag of the current builder.
    /// </summary>
    private HtmlBuilderTag? Tag { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlBuilder"/> class.
    /// </summary>
    public HtmlBuilder()
    {
        _attributes = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        _children = [];
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
        return ParseHtmlAsString(0);
    }
    
    /// <summary>
    /// Gets current builder HTML.
    /// </summary>
    /// <param name="indentHtml">Generate html with indentation?</param>
    public string ToString(bool indentHtml)
    {
        var tabCount = indentHtml ? 1 : 0;
        return ParseHtmlAsString(tabCount);
    }

    private string ParseHtmlAsString(int tabCount)
    {
        var html = new StringBuilder();
        
        if (tabCount > 0 && !_hasRawText)
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
        html.Append(Tag.TagName.GetTagName());
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
            
            if (tabCount > 0 && !_hasRawText)
                html.AppendLine().Append(' ', tabCount * 2);
        }
       
        html.Append("</");
        html.Append(Tag.TagName.GetTagName());
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
            content.Append(child.ParseHtmlAsString(tabCount));
        }
        return content.ToString();
    }

    private string GetAttributesHtml()
    {
        var attributes = new StringBuilder();
        foreach (var item in _attributes)
        {
            attributes.AppendFormat(" {0}=\"{1}\"", item.Key, item.Value);
        }
        
        return attributes.ToString();
    }
    
    public string GetAttribute(string key)
    {
        return _attributes[key];
    }
}
