using System;
using System.Collections.Generic;
using JJMasterData.Core.Web.Components;

namespace JJMasterData.Core.Web.Html;

public partial class HtmlBuilder
{
    /// <summary>
    /// Insert a HTML builder as a child of caller builder.
    /// </summary>
    public HtmlBuilder AppendElement(HtmlBuilder builder)
    {
        if (builder != null)
            _children.Add(builder);

        return this;
    }

    /// <summary>
    /// Insert a list of HTML builder as a child of caller builder.
    /// </summary>
    public HtmlBuilder AppendRange(IEnumerable<HtmlBuilder> htmlList)
    {
        if (htmlList == null) 
            return this;
            
        foreach (var item in htmlList)
            AppendElement(item);

        return this;
    }

    /// <summary>
    /// Insert a HTML builder as a child of caller builder.
    /// </summary>
    public HtmlBuilder AppendElement(HtmlTag tag, Action<HtmlBuilder> elementAction = null)
    {
        var childElement = new HtmlBuilder(tag);
        elementAction?.Invoke(childElement);
        AppendElement(childElement);
        return this;
    }
        
    /// <summary>
    /// Conditional insert a HTML builder from a return of a function. Use this to improve performance.
    /// </summary>
    public HtmlBuilder AppendElementIf(bool condition, Func<HtmlBuilder> func)
    {
        if (condition)
            AppendElement(func.Invoke());

        return this;
    }

    /// <summary>
    /// Conditional insert HTML builder as a child of caller builder.
    /// </summary>
    public HtmlBuilder AppendElementIf(bool condition, HtmlTag tag, Action<HtmlBuilder> elementAction = null)
    {
        if (condition)
            AppendElement(tag, elementAction);

        return this;
    }

    /// <summary>
    /// Insert raw text as a child of caller builder.
    /// </summary>
    public HtmlBuilder AppendText(string rawText)
    {
        if (!string.IsNullOrEmpty(rawText))
        {
            var childElement = new HtmlBuilder(rawText);
            AppendElement(childElement);
        }
                
        return this;
    }

    /// <summary>
    /// Insert string representation of the integer as a child of caller builder.
    /// </summary>
    public HtmlBuilder AppendText(int value)
    {
        var childElement = new HtmlBuilder(value.ToString());
        AppendElement(childElement);
        return this;
    }

    /// <summary>
    /// Conditional insert raw text as a child of caller builder.
    /// </summary>
    public HtmlBuilder AppendTextIf(bool condition, string rawText)
    {
        if (condition)
            AppendText(rawText);

        return this;
    }

    /// <summary>
    /// Append a hidden input to the Element tree.
    /// </summary>
    public HtmlBuilder AppendHiddenInput(string name, string value)
    {
        return AppendElement(HtmlTag.Input, input =>
        {
            input.WithAttribute("hidden", "hidden");
            input.WithNameAndId(name);
            input.WithValue(value);
        });
    }

    /// <summary>
    /// Append a hidden input to the Element tree.
    /// </summary>
    public HtmlBuilder AppendHiddenInput(string name)
    {
        return AppendHiddenInput(name, string.Empty);
    }

    /// <summary>
    /// Insert a script tag with a rawScript
    /// </summary>
    public HtmlBuilder AppendScript(string rawScript)
    {
        var childElement = new HtmlBuilder(HtmlTag.Script)
            .WithAttribute("type", "text/javascript")
            .AppendText(rawScript);

        return AppendElement(childElement);
    }

    /// <summary>
    /// Insert a JJ component as a child of caller builder.
    /// </summary>
    public HtmlBuilder AppendElement(JJBaseView component)
    {
        if (component != null)
            AppendElement(component.GetHtmlBuilder());

        return this;
    }

}