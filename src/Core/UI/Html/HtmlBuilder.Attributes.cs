#nullable enable

using System.Collections.Generic;
using System.Web;
using JetBrains.Annotations;

namespace JJMasterData.Core.UI.Html;

public partial class HtmlBuilder
{
    /// <summary>
    /// Set HTML builder name and ID.
    /// </summary>
    public HtmlBuilder WithNameAndId(string? id)
    {
        if (id != null && !string.IsNullOrWhiteSpace(id))
            WithId(id).WithName(id);

        return this;
    }
    
    public HtmlBuilder WithId(string id)
    {
        WithAttribute("id", id);

        return this;
    }
    
    public HtmlBuilder WithName(string name)
    {
        WithAttribute("name", name);

        return this;
    }

    /// <summary>
    /// Set attribute to the HTML builder.
    /// </summary>
    public HtmlBuilder WithAttribute(string name, string value)
    {
        _attributes[name] = value;
        return this;
    }

    public HtmlBuilder WithAttribute(string name, int value)
    {
        _attributes[name] = value.ToString();
        return this;
    }

    /// <summary>
    /// Set attribute to the HTML builder on condition.
    /// </summary>
    public HtmlBuilder WithAttributeIf(bool condition, string name, string value)
    {
        if (condition)
            WithAttribute(name, value);

        return this;
    }

    /// <summary>
    /// Set attribute to the HTML builder on condition.
    /// </summary>
    public HtmlBuilder WithAttributeIfNotEmpty(string name, string? value)
    {
        if (value != null && !string.IsNullOrEmpty(value))
            WithAttribute(name, value);

        return this;
    }
    
    /// <summary>
    /// Set attribute to the HTML builder.
    /// </summary>
    public HtmlBuilder WithSingleAttribute(string nameAndValue)
    {
        return WithAttribute(nameAndValue, nameAndValue);
    }

    /// <summary>
    /// Set attribute to the HTML builder on condition.
    /// </summary>
    public HtmlBuilder WithAttributeIf(bool condition, string nameAndValue)
    {
        return WithAttributeIf(condition, nameAndValue, nameAndValue);
    }
    
    /// <summary>
    /// Set CSS classes attributes, if already exists it will be ignored.
    /// </summary>
    public HtmlBuilder WithCssClass(string? classes)
    {
        if (classes == null || string.IsNullOrWhiteSpace(classes))
            return this;

        if (!_attributes.ContainsKey("class"))
            return WithAttribute("class", classes);

        var classList = new List<string>();
        classList.AddRange(_attributes["class"].Split(' '));
        
        foreach (var cssClass in classes.Split(' '))
        {
            if (!classList.Contains(cssClass))
                classList.Add(cssClass);
        }

        _attributes["class"] = string.Join(" ", classList);

        return this;
    }

    /// <summary>
    /// Conditional to set classes attributes, if already exists it will be ignored.
    /// </summary>
    public HtmlBuilder WithCssClassIf(bool conditional, string? classes)
    {
        if (conditional)
            WithCssClass(classes);

        return this;
    }

    /// <summary>
    /// Set range of attrs
    /// </summary>
    internal HtmlBuilder WithAttributes(Dictionary<string, string> attributes)
    {
        foreach (var v in attributes)
        {
            _attributes.Add(v.Key, v.Value);
        }

        return this;
    }

    /// <summary>
    /// Sets a tooltip to the HTML Tag
    /// </summary>
    public HtmlBuilder WithToolTip(string? tooltip)
    {
        if (tooltip != null && !string.IsNullOrEmpty(tooltip))
        {
            _attributes["title"] = HttpUtility.HtmlAttributeEncode(tooltip);
            _attributes[BootstrapHelper.DataToggle] = "tooltip";
        }

        return this;
    }

    /// <summary>
    /// Set custom data attribute to HTML builder.
    /// </summary>
    public HtmlBuilder WithValue(string @value)
    {
        return WithAttribute("value", @value);
    }

    /// <summary>
    /// Set a custom Bootstrap data attribute to HTML builder.
    /// </summary>
    public HtmlBuilder WithDataAttribute(string name, string value)
    {
        var attributeName = BootstrapHelper.Version >= 5 ? "data-bs-" : "data-";
        attributeName += name;
        return WithAttribute(attributeName, value);
    }
    
    public HtmlBuilder WithOnChange([LanguageInjection("JavaScript")]string value)
    {
        _attributes["onchange"] = value;
        return this;
    }
    
    public HtmlBuilder WithOnClick([LanguageInjection("JavaScript")]string value)
    {
        _attributes["onclick"] = value;
        return this;
    }
    
    public HtmlBuilder WithStyle(string value)
    {
        _attributes["style"] = value;
        return this;
    }
}