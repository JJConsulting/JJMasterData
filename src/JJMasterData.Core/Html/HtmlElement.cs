using JJMasterData.Core.WebComponents;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace JJMasterData.Core.Html;

/// <summary>
/// Implementation of HTML element.
/// </summary>
public class HtmlElement
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
    /// Insert HTML element as a child of caller element.
    /// </summary>
    public HtmlElement AppendElement(HtmlElement element)
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        _children.Add(element);
        return this;
    }

    /// <summary>
    /// Insert a list of HTML element as a child of caller element.
    /// </summary>
    public HtmlElement AppendRange(IList<HtmlElement> listelement)
    {
        if (listelement != null)
        {
            foreach (var item in listelement)
                AppendElement(item);
        }

        return this;
    }

    /// <summary>
    /// Insert HTML element as a child of caller element.
    /// </summary>
    public HtmlElement AppendElement(HtmlTag tag, Action<HtmlElement> elementAction = null)
    {
        var childElement = new HtmlElement(tag);
        elementAction?.Invoke(childElement);
        _children.Add(childElement);
        return this;
    }

    public HtmlElement AppendElementIf(bool condition, HtmlElement htmlElement = null)
    {
        if (condition)
            AppendElement(htmlElement);

        return this;
    }

    /// <summary>
    /// Conditional insert HTML element as a child of caller element.
    /// </summary>
    public HtmlElement AppendElementIf(bool condition, HtmlTag tag, Action<HtmlElement> elementAction = null)
    {
        if (condition)
            AppendElement(tag, elementAction);

        return this;
    }

    /// <summary>
    /// Insert raw text as a child of caller element.
    /// </summary>
    /// <param name="rawText"></param>
    /// <returns></returns>
    public HtmlElement AppendText(string rawText)
    {
        var childElement = new HtmlElement(rawText);
        _children.Add(childElement);
        return this;
    }


    /// <summary>
    /// Conditional insert raw text as a child of caller element.
    /// </summary>
    public HtmlElement AppendTextIf(bool condition, string rawText)
    {
        if (condition)
            AppendText(rawText);

        return this;
    }

    /// <summary>
    /// Set HTML element name and ID.
    /// </summary>
    public HtmlElement WithNameAndId(string id)
    {
        if (!string.IsNullOrWhiteSpace(id))
            WithAttribute("id", id).WithAttribute("name", id);

        return this;
    }

    /// <summary>
    /// Set attribute to the HTML element.
    /// </summary>
    public HtmlElement WithAttribute(string name, string value)
    {
        _attributes.Add(name, value);
        return this;
    }


    /// <summary>
    /// Set Title to the HTML element.
    /// </summary>
    public HtmlElement WithToolTip(string tooltip)
    {
        if (!string.IsNullOrEmpty(tooltip))
        {
            if (_attributes.ContainsKey("title"))
                _attributes["title"] = tooltip;
            else
                _attributes.Add("title", tooltip);

            if (_attributes.ContainsKey(BootstrapHelper.DataToggle))
                _attributes[BootstrapHelper.DataToggle] = "tooltip";
            else
                _attributes.Add(BootstrapHelper.DataToggle, "tooltip");
        }

        return this;
    }


    /// <summary>
    /// Set attribute to the HTML element on condition.
    /// </summary>
    public HtmlElement WithAttributeIf(bool condition, string name, string value)
    {

        if (condition)
            _attributes.Add(name, value);

        return this;
    }


    /// <summary>
    /// Set classes attributes, if already exists will be ignored.
    /// </summary>
    public HtmlElement WithCssClass(string classes)
    {
        if (string.IsNullOrWhiteSpace(classes))
            return this;

        if (!_attributes.ContainsKey("class"))
            return WithAttribute("class", classes);

        var listClass = new List<string>();
        listClass.AddRange(_attributes["class"].Split(' '));
        foreach (string cssClass in classes.Split(' '))
        {
            if (!listClass.Contains(cssClass))
                listClass.Add(cssClass);
        }

        _attributes["class"] = string.Join(" ", listClass);

        return this;
    }


    /// <summary>
    /// Conditional to set classes attributes, if already exists will be ignored.
    /// </summary>
    public HtmlElement WithCssClassIf(bool conditional, string classes)
    {
        if (conditional)
            WithCssClass(classes);

        return this;
    }

    /// <summary>
    /// Set custom data attribute to HTML element.
    /// </summary>
    public HtmlElement WithDataAttribute(string name, string value)
    {
        return WithAttribute($"data-{name}", value);
    }

    /// <summary>
    /// Set range of attrs
    /// </summary>
    internal HtmlElement WithAttributes(Hashtable attributes)
    {
        foreach (DictionaryEntry v in attributes)
        {
            _attributes.Add(v.Key.ToString(), v.Value.ToString());
        }

        return this;
    }

    /// <summary>
    /// Gets current element HTML.
    /// </summary>
    internal string GetElementHtml(int tabCount)
    {
        var html = new StringBuilder();
        if (tabCount > 0)
            html.AppendLine("").Append('\t', tabCount);

        if (_hasRawText || Tag == null)
        {
            html.Append(_rawText);
            return html.ToString();
        }

        html.Append("<");
        html.Append(Tag.TagName.ToString().ToLower());
        html.Append(GetHtmlAttrs());

        if (!Tag.HasClosingTag)
        {
            html.Append("/>");
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

    private string GetHtmlAttrs()
    {
        var attrs = new StringBuilder();
        foreach (var item in _attributes)
        {
            attrs.AppendFormat(" {0}=\"{1}\"", item.Key, item.Value);
        }

        return attrs.ToString();
    }

}
