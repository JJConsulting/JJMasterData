#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.ObjectPool;

namespace JJMasterData.Core.UI.Html;

/// <summary>
/// Represents a mutable string of HTML tags.
/// </summary>
/// <example>
/// [!include[Example](../../../doc/Documentation/articles/usages/htmlbuilder.md)]
/// </example>
public sealed partial class HtmlBuilder
{
    private readonly string? _rawText;
    private readonly bool _hasRawText;
    private readonly Dictionary<string, string> _attributes;
    private readonly List<HtmlBuilder?> _children;
    private readonly HtmlBuilderTag? _tag;
    
    private static readonly ObjectPool<StringBuilder> StringBuilderPool;
    
    static HtmlBuilder()
    {
        StringBuilderPool = new DefaultObjectPoolProvider().CreateStringBuilderPool();
    }
    
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
        _tag = new HtmlBuilderTag(tag);
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
    /// <param name="indentHtml">Generate html with indentation if true.</param>
    public string ToString(bool indentHtml)
    {
        var tabCount = indentHtml ? 1 : 0;
        return ParseHtmlAsString(tabCount);
    }

    private string ParseHtmlAsString(int tabCount)
    {
        string html;
        
        var htmlBuilder = StringBuilderPool.Get();
        
        if (tabCount > 0 && !_hasRawText)
        {
            htmlBuilder.AppendLine().Append(' ', tabCount * 2);
        }

        if (_hasRawText || _tag is null)
        {
            htmlBuilder.Append(_rawText);
            htmlBuilder.Append(GetHtmlContent(tabCount));

            html = htmlBuilder.ToString();

            StringBuilderPool.Return(htmlBuilder);
        
            return html;
        }

        htmlBuilder.Append('<');
        htmlBuilder.Append(_tag.GetTagName());
        htmlBuilder.Append(GetAttributesHtml());

        if (!_tag.HasClosingTag)
        {
            htmlBuilder.Append(" />");
           
            html = htmlBuilder.ToString();

            StringBuilderPool.Return(htmlBuilder);
        
            return html;
        }

        htmlBuilder.Append('>');

        if (_tag.HtmlTag is HtmlTag.TextArea)
        {
            htmlBuilder.Append(GetHtmlContent(0));
        }
        else
        {
            htmlBuilder.Append(GetHtmlContent(tabCount));
            
            if (tabCount > 0 && !_hasRawText)
                htmlBuilder.AppendLine().Append(' ', tabCount * 2);
        }
       
        htmlBuilder.Append("</");
        htmlBuilder.Append(_tag.GetTagName());
        htmlBuilder.Append('>');

        html = htmlBuilder.ToString();

        StringBuilderPool.Return(htmlBuilder);
        
        return html;
    }

    private string GetHtmlContent(int tabCount)
    {
        if (tabCount > 0)
            tabCount++;

        var contentBuilder = StringBuilderPool.Get();
        foreach (var child in _children)
        {
            contentBuilder.Append(child?.ParseHtmlAsString(tabCount));
        }
        var content = contentBuilder.ToString();
        
        StringBuilderPool.Return(contentBuilder);

        return content;
    }

    private string GetAttributesHtml()
    {
        var attributesBuilder = StringBuilderPool.Get();
        foreach (var item in _attributes)
        {
            attributesBuilder.Append($" {item.Key}=\"{item.Value}\"");
        }
        
        var attributes = attributesBuilder.ToString();
        
        StringBuilderPool.Return(attributesBuilder);

        return attributes;
    }
    
    public string GetAttribute(string key)
    {
        return _attributes[key];
    }
}
