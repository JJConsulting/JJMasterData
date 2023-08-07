﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Core.Web.Components;

namespace JJMasterData.Core.Web.Html;

public partial class HtmlBuilder
{
    /// <summary>
    /// Insert a HTML builder as a child of caller builder.
    /// </summary>
    public HtmlBuilder Append(HtmlBuilder builder)
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
            Append(item);

        return this;
    }
    
    public async Task<HtmlBuilder> AppendRangeAsync(IAsyncEnumerable<HtmlBuilder> htmlList)
    {
        if (htmlList == null) 
            return this;
            
        await foreach (var item in htmlList)
            Append(item);

        return this;
    }

    /// <summary>
    /// Insert a HTML builder as a child of caller builder.
    /// </summary>
    public HtmlBuilder Append(HtmlTag tag, Action<HtmlBuilder> builderAction = null)
    {
        var child = new HtmlBuilder(tag);
        builderAction?.Invoke(child);
        Append(child);
        return this;
    }
    
    public async Task<HtmlBuilder> AppendAsync(HtmlTag tag, Func<HtmlBuilder,Task> builderAction = null)
    {
        var child = new HtmlBuilder(tag);
        await builderAction?.Invoke(child)!;
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

    
    /// <summary>
    /// Conditional insert HTML builder as a child of caller builder.
    /// </summary>
    public HtmlBuilder AppendIf(bool condition, HtmlTag tag, Action<HtmlBuilder> builderAction = null)
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
    /// Append a hidden input to the Element tree.
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
        var child = new HtmlBuilder(HtmlTag.Script)
            .WithAttribute("type", "text/javascript")
            .AppendText(rawScript);

        return Append(child);
    }

    /// <summary>
    /// Insert a JJ component as a child of caller builder.
    /// </summary>
    public HtmlBuilder AppendComponent(JJBaseView component)
    {
        if (component != null)
            Append(component.GetHtmlBuilder());

        return this;
    }
    
    public async Task<HtmlBuilder> AppendComponentAsync(JJAsyncBaseView component)
    {
        if (component != null)
            Append(await component.GetHtmlBuilderAsync());

        return this;
    }

}