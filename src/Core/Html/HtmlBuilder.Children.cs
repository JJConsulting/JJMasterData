#nullable enable

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using JJMasterData.Core.UI.Components;

namespace JJMasterData.Core.UI.Html;

public partial class HtmlBuilder
{
    public HtmlBuilder Prepend(HtmlBuilder? builder)
    {
        if (builder != null)
            _children.Insert(0, builder);

        return this;
    }

    public HtmlBuilder PrependComponent(HtmlComponent? component)
    {
        if (component != null)
            _children.Insert(0, component.GetHtmlBuilder());

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
        _children.AddRange(htmlEnumerable);

        return this;
    }

    /// <summary>
    /// Appends a new HTML element with the specified tag to the current builder.
    /// </summary>
    /// <param name="tag">The HTML tag representing the element to be appended.</param>
    /// <param name="builderAction">An optional action used to configure the child HTML builder.</param>
    /// <returns>The current <see cref="HtmlBuilder"/> instance with the new element appended.</returns>
    public HtmlBuilder Append(HtmlTag tag, [InstantHandle] Action<HtmlBuilder>? builderAction = null)
    {
        var child = new HtmlBuilder(tag);
        builderAction?.Invoke(child);
        Append(child);
        return this;
    }

    /// <summary>
    /// Appends a new HTML element with the specified tag to the current builder, using a stateful configuration.
    /// </summary>
    /// <typeparam name="TState">The type of the external state object.</typeparam>
    /// <param name="tag">The HTML tag representing the element to be appended.</param>
    /// <param name="state">The external state passed to the builder action.</param>
    /// <param name="builderAction">A static delegate that configures the child HTML builder using the provided state.</param>
    /// <returns>The current <see cref="HtmlBuilder"/> instance with the new element appended.</returns>
    public HtmlBuilder Append<TState>(
        HtmlTag tag,
        TState state,
        [InstantHandle, RequireStaticDelegate] Action<TState, HtmlBuilder> builderAction)
    {
        var child = new HtmlBuilder(tag);
        builderAction(state, child);
        Append(child);
        return this;
    }
    
    /// <summary>
    /// Appends a new &lt;div&gt; element to the current builder.
    /// </summary>
    /// <param name="builderAction">An optional action used to configure the child HTML builder.</param>
    /// <returns>The current <see cref="HtmlBuilder"/> instance with the new &lt;div&gt; element appended.</returns>
    public HtmlBuilder AppendDiv([InstantHandle] Action<HtmlBuilder>? builderAction = null)
    {
        return Append(HtmlTag.Div, builderAction);
    }

    /// <summary>
    /// Appends a new &lt;div&gt; element to the current builder using a stateful configuration.
    /// </summary>
    /// <typeparam name="TState">The type of the external state object.</typeparam>
    /// <param name="state">The external state passed to the builder action.</param>
    /// <param name="builderAction">A static delegate that configures the child HTML builder using the provided state.</param>
    /// <returns>The current <see cref="HtmlBuilder"/> instance with the new &lt;div&gt; element appended.</returns>
    public HtmlBuilder AppendDiv<TState>(
        TState state,
        [InstantHandle, RequireStaticDelegate] Action<TState, HtmlBuilder> builderAction)
    {
        return Append(HtmlTag.Div, state, builderAction);
    }

    /// <summary>
    /// Appends a new &lt;span&gt; element to the current builder.
    /// </summary>
    /// <param name="builderAction">An optional action used to configure the child HTML builder.</param>
    /// <returns>The current <see cref="HtmlBuilder"/> instance with the new &lt;span&gt; element appended.</returns>
    public HtmlBuilder AppendSpan([InstantHandle] Action<HtmlBuilder>? builderAction = null)
    {
        return Append(HtmlTag.Span, builderAction);
    }

    /// <summary>
    /// Appends a new &lt;span&gt; element to the current builder using a stateful configuration.
    /// </summary>
    /// <typeparam name="TState">The type of the external state object.</typeparam>
    /// <param name="state">The external state passed to the builder action.</param>
    /// <param name="builderAction">A static delegate that configures the child HTML builder using the provided state.</param>
    /// <returns>The current <see cref="HtmlBuilder"/> instance with the new &lt;span&gt; element appended.</returns>
    public HtmlBuilder AppendSpan<TState>(TState state,
        [InstantHandle, RequireStaticDelegate] Action<TState, HtmlBuilder> builderAction)
    {
        return Append(HtmlTag.Span, state, builderAction);
    }

    /// <summary>
    /// Appends a new &lt;input&gt; element to the current builder.
    /// </summary>
    /// <param name="builderAction">An optional action used to configure the child HTML builder.</param>
    /// <returns>The current <see cref="HtmlBuilder"/> instance with the new &lt;input&gt; element appended.</returns>
    public HtmlBuilder AppendInput([InstantHandle] Action<HtmlBuilder>? builderAction = null)
    {
        return Append(HtmlTag.Input, builderAction);
    }

    /// <summary>
    /// Appends a new &lt;input&gt; element to the current builder using a stateful configuration.
    /// </summary>
    /// <typeparam name="TState">The type of the external state object.</typeparam>
    /// <param name="state">The external state passed to the builder action.</param>
    /// <param name="builderAction">A static delegate that configures the child HTML builder using the provided state.</param>
    /// <returns>The current <see cref="HtmlBuilder"/> instance with the new &lt;input&gt; element appended.</returns>
    public HtmlBuilder AppendInput<TState>(TState state,
        [InstantHandle, RequireStaticDelegate] Action<TState, HtmlBuilder> builderAction)
    {
        return Append(HtmlTag.Input, state, builderAction);
    }

    /// <summary>
    /// Appends a new &lt;label&gt; element to the current builder.
    /// </summary>
    /// <param name="builderAction">An optional action used to configure the child HTML builder.</param>
    /// <returns>The current <see cref="HtmlBuilder"/> instance with the new &lt;label&gt; element appended.</returns>
    public HtmlBuilder AppendLabel([InstantHandle] Action<HtmlBuilder>? builderAction = null)
    {
        return Append(HtmlTag.Label, builderAction);
    }

    /// <summary>
    /// Appends a new &lt;label&gt; element to the current builder using a stateful configuration.
    /// </summary>
    /// <typeparam name="TState">The type of the external state object.</typeparam>
    /// <param name="state">The external state passed to the builder action.</param>
    /// <param name="builderAction">A static delegate that configures the child HTML builder using the provided state.</param>
    /// <returns>The current <see cref="HtmlBuilder"/> instance with the new &lt;label&gt; element appended.</returns>
    public HtmlBuilder AppendLabel<TState>(TState state,
        [InstantHandle, RequireStaticDelegate] Action<TState, HtmlBuilder> builderAction)
    {
        return Append(HtmlTag.Label, state, builderAction);
    }


    public HtmlBuilder AppendHr()
    {
        return Append(HtmlTag.Hr);
    }

    public HtmlBuilder AppendBr()
    {
        var child = new HtmlBuilder(HtmlTag.Br);
        Append(child);
        return this;
    }

    public HtmlBuilder AppendLink(string text, string link)
    {
        var child = new HtmlBuilder(HtmlTag.A)
            .AppendText(text)
            .WithAttribute("href", link);

        Append(child);

        return this;
    }

    /// <summary>
    /// Conditional insert a HTML builder from a return of a function. Use this to improve performance.
    /// </summary>
    public HtmlBuilder AppendIf(bool condition, [InstantHandle] Func<HtmlBuilder> func)
    {
        if (condition)
            Append(func.Invoke());

        return this;
    }

    /// <summary>
    /// Conditional insert HTML builder as a child of caller builder.
    /// </summary>
    public HtmlBuilder AppendIf(bool condition, HtmlTag tag, [InstantHandle] Action<HtmlBuilder>? builderAction = null)
    {
        if (condition)
            Append(tag, builderAction);

        return this;
    }

    /// <summary>
    /// Insert raw text as a child of caller builder.
    /// </summary>
    public HtmlBuilder AppendText(string? rawText)
    {
        if (!string.IsNullOrEmpty(rawText))
        {
            var child = new HtmlBuilder(rawText!);
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
    public HtmlBuilder AppendTextIf(bool condition, string? rawText)
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
        return Append(HtmlTag.Input, (name, value), static (state, input) =>
        {
            input.WithAttribute("hidden", "hidden");
            input.WithNameAndId(state.name);
            input.WithValue(state.value);
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
    public HtmlBuilder AppendScript([LanguageInjection("javascript")] string rawScript)
    {
        var child = new HtmlBuilder(HtmlTag.Script)
            .WithAttribute("type", "text/javascript")
            .AppendText(rawScript);

        return Append(child);
    }

    /// <summary>
    /// Insert a script tag with a rawScript
    /// </summary>
    public HtmlBuilder AppendScriptIf(bool condition, [LanguageInjection("javascript")] string rawScript)
    {
        var child = new HtmlBuilder(HtmlTag.Script)
            .WithAttribute("type", "text/javascript")
            .AppendText(rawScript);

        return condition ? Append(child) : this;
    }

    /// <summary>
    /// Insert a <see cref="HtmlComponent"/> as a child of caller builder.
    /// </summary>
    public HtmlBuilder AppendComponent(HtmlComponent? component)
    {
        if (component is not null)
            Append(component.GetHtmlBuilder());

        return this;
    }
}