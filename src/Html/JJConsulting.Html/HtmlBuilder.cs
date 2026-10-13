using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace JJConsulting.Html;

/// <summary>
/// A utility class for building HTML structures programmatically.
/// Provides methods for creating, combining, and rendering HTML strings with nested elements, attributes,
/// and raw text support.
/// </summary>
public class HtmlBuilder : IHtmlContent
{
    private readonly Dictionary<string, string?>? _attributes;
    private readonly List<IHtmlContent?> _children;
    private readonly HtmlTag? _tag;

    public HtmlBuilder()
    {
        _children = [];
    }

    public HtmlBuilder(string rawText, bool encode = true) : this()
    {
        _children.Add(new HtmlText(rawText, encode));
    }

    public HtmlBuilder(HtmlTag tag) : this()
    {
        _tag = tag;
        _attributes = new Dictionary<string, string?>(StringComparer.InvariantCultureIgnoreCase);
    }

    public HtmlBuilder(HtmlTag tag, string rawText) : this(tag)
    {
        _children.Add(new HtmlText(rawText));
    }

    public HtmlBuilder(HtmlTag tag, params List<IHtmlContent?> children)
    {
        _tag = tag;
        _children = children;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        using var writer = new StringWriter(sb);

        WriteTo(writer, HtmlEncoder.Default);

        var result = sb.ToString();

        return result;
    }

    public void WriteTo(TextWriter writer, HtmlEncoder encoder)
    {
        if (!_tag.HasValue)
        {
            foreach (var c in _children)
            {
                c?.WriteTo(writer, encoder);
            }

            return;
        }

        var tag = _tag.Value;
        var tagName = tag.Name;

        writer.Write('<');
        writer.Write(tagName);

        WriteAttributes(writer, encoder);

        if (!tag.HasClosingTag)
        {
            writer.Write(" />");
            return;
        }

        writer.Write('>');

        foreach (var child in _children)
        {
            child?.WriteTo(writer, encoder);
        }

        writer.Write("</");
        writer.Write(tagName);
        writer.Write('>');
    }

    private void WriteAttributes(TextWriter writer, HtmlEncoder encoder)
    {
        if (_attributes?.Count is null or 0)
            return;

        foreach (var attribute in _attributes)
        {
            writer.Write(' ');
            writer.Write(attribute.Key);

            if (attribute.Value is null)
                continue;

            writer.Write("=\"");
            encoder.Encode(writer, attribute.Value);
            writer.Write('"');
        }
    }

    public string? GetAttribute(string key) => _attributes?[key];

    public bool TryGetAttribute(string key, out string? value)
    {
        if (_attributes is not null)
            return _attributes.TryGetValue(key, out value);

        value = null;

        return false;
    }

    public HtmlBuilder WithAttribute(string name, string? value = null)
    {
        _attributes?[name] = value;
        return this;
    }

    public HtmlBuilder Prepend(IHtmlContent? html)
    {
        if (html is null)
            return this;

        if (ReferenceEquals(this, html))
            throw new InvalidOperationException("Cannot prepend the same HtmlBuilder instance to itself.");

        _children.Insert(0, html);

        return this;
    }

    public HtmlBuilder Append(IHtmlContent? html)
    {
        if (html is null)
            return this;

        if (ReferenceEquals(this, html))
            throw new InvalidOperationException("Cannot append the same HtmlBuilder instance to itself.");

        _children.Add(html);

        return this;
    }

    public HtmlBuilder AppendRange(IEnumerable<IHtmlContent> htmlEnumerable)
    {
        _children.AddRange(htmlEnumerable);
        return this;
    }
}