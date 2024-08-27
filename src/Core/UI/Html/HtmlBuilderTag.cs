using System;

namespace JJMasterData.Core.UI.Html;

/// <summary>
/// Represents a HTML tag of a <see cref="HtmlBuilder"/>
/// </summary>
public sealed class HtmlBuilderTag(HtmlTag htmlTag)
{
    public HtmlTag HtmlTag { get; } = htmlTag;

    public bool HasClosingTag { get; } = htmlTag switch
    {
        HtmlTag.Area or HtmlTag.Br or HtmlTag.Hr or HtmlTag.Img or HtmlTag.Input => false,
        _ => true
    };

    public string GetTagName() => HtmlTag switch
    {
        HtmlTag.A => "a",
        HtmlTag.B => "b",
        HtmlTag.Blockquote => "blockquote",
        HtmlTag.Area => "area",
        HtmlTag.Br => "br",
        HtmlTag.Div => "div",
        HtmlTag.Span => "span",
        HtmlTag.Label => "label",
        HtmlTag.Input => "input",
        HtmlTag.Strong => "strong",
        HtmlTag.Button => "button",
        HtmlTag.H1 => "h1",
        HtmlTag.H2 => "h2",
        HtmlTag.H3 => "h3",
        HtmlTag.H4 => "h4",
        HtmlTag.H5 => "h5",
        HtmlTag.H6 => "h6",
        HtmlTag.Small => "small",
        HtmlTag.Ul => "ul",
        HtmlTag.Li => "li",
        HtmlTag.TextArea => "textarea",
        HtmlTag.Script => "script",
        HtmlTag.Select => "select",
        HtmlTag.Option => "option",
        HtmlTag.Table => "table",
        HtmlTag.Tr => "tr",
        HtmlTag.Th => "th",
        HtmlTag.Td => "td",
        HtmlTag.Thead => "thead",
        HtmlTag.Tbody => "tbody",
        HtmlTag.Hr => "hr",
        HtmlTag.I => "i",
        HtmlTag.Section => "section",
        HtmlTag.P => "p",
        HtmlTag.Footer => "footer",
        HtmlTag.Img => "img",
        HtmlTag.Center => "center",
        HtmlTag.Video => "video",
        HtmlTag.Form => "form",
        HtmlTag.FieldSet => "fieldset",
        HtmlTag.Legend => "legend",
        HtmlTag.U => "u",
        HtmlTag.OptGroup => "optgroup",
        HtmlTag.Nav => "nav",
        HtmlTag.Ol => "ol",
        HtmlTag.Style => "style",
        HtmlTag.Iframe => "iframe",
        _ => throw new ArgumentOutOfRangeException(nameof(HtmlTag), HtmlTag, "HTML tag not implemented.")
    };
}