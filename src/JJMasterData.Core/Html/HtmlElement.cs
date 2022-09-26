using JJMasterData.Core.WebComponents;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JJMasterData.Core.Html;

/// <summary>
/// Implementation of HTML element.
/// </summary>
public class HtmlElement
{
    private HtmlTag tag;
    private List<HtmlElementAttribute> attributes;
    private string rawText;
    private bool hasRawText;
    private HtmlElementsCollection children;

    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlElement"/> class.
    /// </summary>
    internal HtmlElement()
    {
        this.attributes = new List<HtmlElementAttribute>();
        this.children = new HtmlElementsCollection();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlElement"/> class.
    /// </summary>
    /// <param name="rawText"></param>
    internal HtmlElement(string rawText)
    {
        this.rawText = rawText;
        this.hasRawText = true;
        this.attributes = new List<HtmlElementAttribute>();
        this.children = new HtmlElementsCollection();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlElement"/> class.
    /// </summary>
    /// <param name="tag"></param>
    internal HtmlElement(HtmlTag tag)
    {
        this.tag = tag;
        this.attributes = new List<HtmlElementAttribute>();
        this.children = new HtmlElementsCollection();
    }

    /// <summary>
    /// Tag of the current element.
    /// </summary>
    public HtmlTag Tag => this.tag;

    /// <summary>
    /// List of all attributes for current element.
    /// </summary>
    public IList<HtmlElementAttribute> Attributes => this.attributes;

    private string TagLayout
    {
        get
        {
            if (this.hasRawText)
            {
                return string.Empty;
            }

            return this.tag.HasClosingTag ? Layouts.StartEndTagLayout : Layouts.SingleTagLayout;
        }
    }

    /// <summary>
    /// Start HTML element definition if the element type is not defined.
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    public HtmlElement OpenElement(HtmlTag tag)
    {
        this.tag = tag;
        return this;
    }

    /// <summary>
    /// Insert HTML element as a child of caller element.
    /// </summary>
    /// <param name="elementAction"></param>
    /// <returns></returns>
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
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public HtmlElement WithAttribute(string name, string value)
    {
        if (this.attributes.Any(x => x.Name == name))
        {
            throw new InvalidOperationException($"Attribute {name} cannot be duplicated!");
        }

        this.attributes.Add(new HtmlElementAttribute(name, value));
        return this;
    }


    /// <summary>
    /// Set Title to the HTML element.
    /// </summary>
    public HtmlElement WithToolTip(string tooltip)
    {
        var titleAttr = this.attributes.Find(x => x.Name.Equals("title"));
        if (titleAttr != null)
            Attributes.Remove(titleAttr);

        this.attributes.Add(new HtmlElementAttribute("title", tooltip));

        var toggleAttr = this.attributes.Find(x => x.Name.Equals(BootstrapHelper.DataToggle));
        if (toggleAttr != null)
            Attributes.Remove(toggleAttr);

        this.attributes.Add(new HtmlElementAttribute(BootstrapHelper.DataToggle, "tooltip"));

        return this;
    }


    public HtmlElement WithAttributes(Hashtable attributes)
    {
        foreach (DictionaryEntry v in attributes)
        {
            this.attributes.Add(new (v.Key.ToString(), v.Value.ToString()));
        }
        
        return this;
    }

    /// <summary>
    /// Set attribute to the HTML element on condition.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <param name="condition"></param>
    /// <returns></returns>
    public HtmlElement WithAttributeIf(string name, string value, bool condition)
    {
        if (condition)
        {
            if (this.attributes.Any(x => x.Name == name))
            {
                throw new InvalidOperationException($"Attribute {name} cannot be duplicated!");
            }

            this.attributes.Add(new HtmlElementAttribute(name, value));
        }

        return this;
    }

    /// <summary>
    /// Set classes to the HTML element on condition.
    /// </summary>
    /// <param name="classesOnTrue"></param>
    /// <param name="classesOnFalse"></param>
    /// <param name="condition"></param>
    /// <param name="sharedClasses"></param>
    /// <returns></returns>
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
    /// <param name="classes"></param>
    /// <returns></returns>
    public HtmlElement WithClasses(string classes)
    {
        return this.WithAttribute("class", classes);
    }

    /// <summary>
    /// Set custom data attribute to HTML element.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public HtmlElement WithDataAttribute(string name, string value)
    {
        return this.WithAttribute($"data-{name}", value);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        if (!this.hasRawText)
        {
            return string.Format(this.TagLayout, this.tag.Name, string.Join(string.Empty, this.attributes));
        }

        return this.rawText;
    }

    /// <summary>
    /// Gets current element content.
    /// </summary>
    /// <returns></returns>
    internal string GetElementContent()
    {
        StringBuilder content = new StringBuilder();
        foreach (var child in this.children)
        {
            content.Append(child.GetElementHtml());
        }

        return content.ToString();
    }

    /// <summary>
    /// Gets current element HTML.
    /// </summary>
    /// <returns></returns>
    internal string GetElementHtml()
    {
        if (!this.hasRawText)
        {
            string elementLayout = string.Format(this.TagLayout, this.tag.Name, string.Join(string.Empty, this.attributes));

            return string.Format(elementLayout, this.GetElementContent());
        }

        return this.rawText;
    }
}
