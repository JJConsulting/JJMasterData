#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Core.UI.Components;

namespace JJMasterData.Core.UI.Html;

public partial class HtmlBuilder
{
    
    public HtmlBuilder Prepend(HtmlBuilder? builder)
    {
        if (builder != null)
            _children.Insert(0,builder);

        return this;
    }
    
        
    public HtmlBuilder PrependComponent(HtmlComponent? component)
    {
        if (component != null)
            _children.Insert(0,component.GetHtmlBuilder());

        return this;
    }

    
    /// <summary>
    /// Insert a HTML builder as a child of caller builder.
    /// </summary>
    public HtmlBuilder Append(HtmlBuilder? builder)
    {
        if (builder != null)
            _children.Add(builder);

        return this;
    }

    /// <summary>
    /// Insert a list of HTML builder as a child of caller builder.
    /// </summary>
    public HtmlBuilder AppendRange(IEnumerable<HtmlBuilder> htmlEnumerable)
    {
        
        foreach (var item in htmlEnumerable)
            Append(item);

        return this;
    }
    
    public async Task<HtmlBuilder> AppendRangeAsync(IAsyncEnumerable<HtmlBuilder> htmlAsyncEnumerable)
    {
        
        await foreach (var item in htmlAsyncEnumerable)
            Append(item);

        return this;
    }

    /// <summary>
    /// Insert a HTML builder as a child of caller builder.
    /// </summary>
    public HtmlBuilder Append(HtmlTag tag, Action<HtmlBuilder>? builderAction = null)
    {
        var child = new HtmlBuilder(tag);
        builderAction?.Invoke(child);
        Append(child);
        return this;
    }
    
    public HtmlBuilder AppendBr()
    {
        var child = new HtmlBuilder(HtmlTag.Br);
        Append(child);
        return this;
    }
    
    /// <summary>
    /// Insert a HTML div as a child of caller builder.
    /// </summary>
    public HtmlBuilder AppendDiv(Action<HtmlBuilder>? builderAction = null)
    {
        return Append(HtmlTag.Div, builderAction);
    }
    
    
    public HtmlBuilder AppendSpan(Action<HtmlBuilder>? builderAction = null)
    {
        return Append(HtmlTag.Span, builderAction);
    }
    
    public HtmlBuilder AppendInput(Action<HtmlBuilder>? builderAction = null)
    {
        return Append(HtmlTag.Input, builderAction);
    }
    
    public HtmlBuilder AppendLabel(Action<HtmlBuilder>? builderAction = null)
    {
        return Append(HtmlTag.Label, builderAction);
    }
    
    public HtmlBuilder AppendLink(string text, string link)
    {
        var child = new HtmlBuilder(HtmlTag.A)
            .AppendText(text)
            .WithAttribute("href", link);

        Append(child);
        
        return this;
    }
    
    public async Task<HtmlBuilder> AppendAsync(Func<Task<HtmlBuilder>> builderFunc)
    {
        var html = await builderFunc(); 
        Append(html);
        return this;
    }
    
    public async Task<HtmlBuilder> AppendAsync(HtmlTag tag, Func<HtmlBuilder,Task> builderFunc)
    {
        var child = new HtmlBuilder(tag);
        await builderFunc.Invoke(child);
        Append(child);
        return this;
    }
        
    /// <summary>
    /// Conditional insert a HTML builder from a return of a function. Use this to improve performance.
    /// </summary>
    public HtmlBuilder AppendIf(bool condition, Func<HtmlBuilder> func)
    {
        if (condition)
            Append(func.Invoke());

        return this;
    }
    
    public async Task AppendIfAsync(bool condition, Func<Task<HtmlBuilder>> func)
    {
        if (condition)
        {
            Append(await func.Invoke());
        }
    }
    
    /// <summary>
    /// Conditional insert HTML builder as a child of caller builder.
    /// </summary>
    public HtmlBuilder AppendIf(bool condition, HtmlTag tag, Action<HtmlBuilder>? builderAction = null)
    {
        if (condition)
            Append(tag, builderAction);

        return this;
    }

    /// <summary>
    /// Insert raw text as a child of caller builder.
    /// </summary>
    public HtmlBuilder AppendText(string rawText)
    {
        if (!string.IsNullOrEmpty(rawText))
        {
            var child = new HtmlBuilder(rawText);
            Append(child);
        }
                
        return this;
    }

    /// <summary>
    /// Insert string representation of the integer as a child of caller builder.
    /// </summary>
    public HtmlBuilder AppendText(int value)
    {
        var child = new HtmlBuilder(value.ToString());
        Append(child);
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
    /// Append a hidden input to the element tree.
    /// </summary>
    public HtmlBuilder AppendHiddenInput(string name, string value)
    {
        return Append(HtmlTag.Input, input =>
        {
            input.WithAttribute("hidden", "hidden");
            input.WithNameAndId(name);
            input.WithValue(value);
        });
    }

    /// <summary>
    /// Append a hidden input to the element tree.
    /// </summary>
    public HtmlBuilder AppendHiddenInput(string name)
    {
        return AppendHiddenInput(name, string.Empty);
    }

    /// <summary>
    /// Insert a script tag with a raw JavaScript script.
    /// </summary>
    public HtmlBuilder AppendScript(string rawScript)
    {
        var child = new HtmlBuilder(HtmlTag.Script)
            .WithAttribute("type", "text/javascript")
            .AppendText(rawScript);

        return Append(child);
    }
    
    /// <summary>
    /// Insert a script tag with a rawScript
    /// </summary>
    public HtmlBuilder AppendScriptIf(bool condition, string rawScript)
    {
        var child = new HtmlBuilder(HtmlTag.Script)
            .WithAttribute("type", "text/javascript")
            .AppendText(rawScript);

        return condition ? Append(child) : this;
    }

    /// <summary>
    /// Insert a HTMLComponent as a child of caller builder.
    /// </summary>
    public HtmlBuilder AppendComponent(HtmlComponent? component)
    {
        if (component != null)
            Append((HtmlBuilder?)component.GetHtmlBuilder());

        return this;
    }
    
    public HtmlBuilder AppendComponentIf(bool condition, HtmlComponent? component)
    {
        if (condition)
            AppendComponent(component);

        return this;
    }
    
    public async Task<HtmlBuilder> AppendControlAsync(ControlBase? control)
    {
        if (control != null)
            Append(await control.GetHtmlBuilderAsync());

        return this;
    }
}