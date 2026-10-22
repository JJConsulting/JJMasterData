# HtmlBuilder (`JJConsulting.Html`)

`HtmlBuilder` creates HTML programmatically through a fluent API. It is the type returned by many JJMasterData UI components and is useful when an event handler, component, or Tag Helper needs to compose or change markup without building strings manually.

The library is useful when markup depends on runtime data or application logic, when a reusable Tag Helper needs to compose content, or when a small HTML fragment is easier to express in C# than in a separate Razor template. It is not intended to replace Razor for page-sized views.

The package is `JJConsulting.Html`. JJMasterData references it transitively; an application that uses the builder outside JJMasterData can add it explicitly:

```bash
dotnet add package JJConsulting.Html
```

Import the core namespace and its extensions:

```csharp
using JJConsulting.Html;
using JJConsulting.Html.Extensions;
```

## Create and compose elements

Create an element with an `HtmlTag`, configure it, and append children. Every fluent method returns the same builder, so calls can be chained.

```csharp
var card = new HtmlBuilder(HtmlTag.Div)
    .WithCssClass("card")
    .Append(HtmlTag.H2, heading => heading.AppendText("Customer"))
    .Append(HtmlTag.P, paragraph =>
        paragraph.WithCssClass("text-muted")
                 .AppendText("Information loaded from the dictionary."));

var markup = card.ToString();
```

The resulting markup is:

```html
<div class="card"><h2>Customer</h2><p class="text-muted">Information loaded from the dictionary.</p></div>
```

`Append(IHtmlContent?)`, `Prepend(IHtmlContent?)` and `AppendRange(...)` compose existing fragments. `null` passed to `Append` or `Prepend` is ignored. Do not append a builder to itself: this throws `InvalidOperationException` to prevent a recursive tree.

The extension methods also provide `AppendDiv`, `AppendSpan`, `AppendTable`, `AppendTr`, `AppendTd`, `AppendInput` and one `Append<Tag>` method for each member of `HtmlTag`. The generic form is often clearer when nesting several kinds of element; the tag-specific form is convenient for short code:

```csharp
var message = HtmlBuilder.Div()
    .WithCssClass("alert alert-info")
    .AppendStrong(strong => strong.AppendText("Note: "))
    .AppendText("the record has not been saved yet.");
```

`HtmlBuilder.Div()`, `HtmlBuilder.Input()` and the other tag factories are extensions supplied by `JJConsulting.Html.Extensions`. `HtmlTag` contains the supported HTML tags; void tags such as `Input`, `Img`, `Br` and `Hr` render as `<input />`, `<img />`, `<br />` and `<hr />`.

The tag factories are generated from `HtmlTag`, so the factory and `Append<Tag>` methods stay aligned with the supported tag list. A factory can start empty, take encoded text, or take child `IHtmlContent` values:

```csharp
var title = HtmlBuilder.H1("Customer details");
var list = HtmlBuilder.Ul(
    HtmlBuilder.Li("First item"),
    HtmlBuilder.Li("Second item"));

var page = HtmlBuilder.Main(title, list);
```

Use `using JJConsulting.Html.Extensions;` to make the fluent extension methods available. The DSL factories are static extension members on `HtmlBuilder`, so call them with `HtmlBuilder.Div(...)` rather than importing them as ordinary static methods.

## Build a declarative fragment

For markup with a clear tree structure, generated tag factories can make the composition more compact than a sequence of `Append<Tag>` calls:

```csharp
using JJConsulting.Html;
using JJConsulting.Html.Extensions;

var navigation = HtmlBuilder.Nav(
    HtmlBuilder.Ul(
        HtmlBuilder.Li(HtmlBuilder.A("Home").WithHref("/")),
        HtmlBuilder.Li(HtmlBuilder.A("Customers").WithHref("/customers"))))
    .WithCssClass("main-navigation");
```

The DSL and fluent API can be mixed: factories create the tree, and fluent methods add attributes or conditional children. Both produce `HtmlBuilder` instances and can be appended to other `IHtmlContent` fragments.

## Further reading

- [Attributes and conditional output](attributes.md): Configure attributes, CSS classes, and conditional content.
- [Razor, Tag Helpers, and custom helpers](integration.md): Render builders in ASP.NET Core and add application-specific helpers.
- [Text encoding and raw markup](encoding.md): Encode text, render fragments, and append trusted scripts and styles.
