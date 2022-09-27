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
    private HtmlElementTag tag;
    private Dictionary<string, string> attributes;
    private string rawText;
    private bool hasRawText;
    private HtmlElementsCollection children;

    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlElement"/> class.
    /// </summary>
    internal HtmlElement()
    {
        this.attributes = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        this.children = new HtmlElementsCollection();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlElement"/> class.
    /// </summary>
    /// <param name="rawText"></param>
    internal HtmlElement(string rawText) : this()
    {
        this.rawText = rawText;
        this.hasRawText = true;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlElement"/> class.
    /// </summary>
    internal HtmlElement(HtmlTag tag) : this()
    {
        this.tag = new HtmlElementTag(tag);
    }

    /// <summary>
    /// Tag of the current element.
    /// </summary>
    public HtmlElementTag Tag => this.tag;

    /// <summary>
    /// List of all attributes for current element.
    /// </summary>
    public Dictionary<string, string> Attributes => this.attributes;

    private string TagLayout
    {
        get
        {
            if (this.hasRawText)
            {
                return string.Empty;
            }

            return this.tag.HasClosingTag ? "<{0}{1}>{{0}}</{0}>" : "<{0}{1}/>";
        }
    }

    /// <summary>
    /// Start HTML element definition if the element type is not defined.
    /// </summary>
    public HtmlElement OpenElement(HtmlElementTag tag)
    {
        this.tag = tag;
        return this;
    }

    /// <summary>
    /// Insert HTML element as a child of caller element.
    /// </summary>
    public HtmlElement AppendElement(Action<HtmlElement> elementAction)
    {
        var childElement = new HtmlElement();
        elementAction.Invoke(childElement);
        this.children.Add(childElement);
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
        this.children.Add(childElement);
        return this;
    }


    /// <summary>
    /// Conditional insert raw text as a child of caller element.
    /// </summary>
    public HtmlElement AppendTextIf(bool condition, string rawText)
    {
        if (condition)
        {
            var childElement = new HtmlElement(rawText);
            this.children.Add(childElement);
        }

        return this;
    }

    /// <summary>
    /// Conditional insert HTML element as a child of caller element.
    /// </summary>
    public HtmlElement AppendElementIf(bool condition, HtmlTag tag, Action<HtmlElement> elementAction)
    {
        if (condition)
        {
            var childElement = new HtmlElement(tag);
            elementAction.Invoke(childElement);
            this.children.Add(childElement);
        }

        return this;
    }

    /// <summary>
    /// Insert HTML element collection as children of caller element.
    /// </summary>
    public HtmlElement AppendMultiple(Action<HtmlElementsCollection> elementsAction)
    {
        var childElements = new HtmlElementsCollection();
        elementsAction.Invoke(childElements);
        this.children.AddRange(childElements);
        return this;
    }

    /// <summary>
    /// Set attribute to the HTML element.
    /// </summary>
    public HtmlElement WithAttribute(string name, string value)
    {
        
        if (this.attributes.ContainsKey(name))
        {
            throw new InvalidOperationException($"Attribute {name} cannot be duplicated!");
        }

        this.attributes.Add(name, value);
        return this;
    }


    /// <summary>
    /// Set Title to the HTML element.
    /// </summary>
    public HtmlElement WithToolTip(string tooltip)
    {
        if (this.attributes.ContainsKey("title"))
            Attributes["title"] = tooltip;
        else
            Attributes.Add("title", tooltip);

        if (this.attributes.ContainsKey(BootstrapHelper.DataToggle))
            Attributes[BootstrapHelper.DataToggle] = "tooltip";
        else
            Attributes.Add(BootstrapHelper.DataToggle, "tooltip");

        return this;
    }


    public HtmlElement WithAttributes(Hashtable attributes)
    {
        foreach (DictionaryEntry v in attributes)
        {
            this.attributes.Add(v.Key.ToString(), v.Value.ToString());
        }
        
        return this;
    }

    /// <summary>
    /// Set attribute to the HTML element on condition.
    /// </summary>
    public HtmlElement WithAttributeIf(string name, string value, bool condition)
    {
        if (condition)
        {
            this.attributes.Add(name, value);
        }

        return this;
    }

    /// <summary>
    /// Set classes to the HTML element on condition.
    /// </summary>
    public HtmlElement WithConditionalClasses(string classesOnTrue, string classesOnFalse, bool condition, string sharedClasses = "")
    {
        classesOnTrue = classesOnTrue ?? string.Empty;
        classesOnFalse = classesOnFalse ?? string.Empty;

        string classes = $"{(condition ? classesOnTrue.Trim() : classesOnFalse.Trim())} {sharedClasses.Trim()}";

        this.WithClasses(classes.Trim());

        return this;
    }


    /// <summary>
    /// Set HTML classes.
    /// </summary>
    public HtmlElement WithClasses(string classes)
    {
        return this.WithAttribute("class", classes);
    }

    /// <summary>
    /// Set custom data attribute to HTML element.
    /// </summary>
    public HtmlElement WithDataAttribute(string name, string value)
    {
        return this.WithAttribute($"data-{name}", value);
    }

    /// <summary>
    /// Gets current element content.
    /// </summary>
    internal string GetElementContent()
    {
        var content = new StringBuilder();
        foreach (var child in this.children)
        {
            content.Append(child.GetElementHtml());
        }

        return content.ToString();
    }

    /// <summary>
    /// Gets current element HTML.
    /// </summary>
    internal string GetElementHtml()
    {
        if (!this.hasRawText)
        {
            var attrs = new StringBuilder();
            foreach (var item in attributes)
            {
                attrs.AppendFormat(" {0}=\"{1}\"", item.Key, item.Value);
            }

            string elementLayout = string.Format(this.TagLayout, this.tag.TagName, string.Join(string.Empty, attrs.ToString()));

            return string.Format(elementLayout, this.GetElementContent());
        }

        return this.rawText;
    }

}
