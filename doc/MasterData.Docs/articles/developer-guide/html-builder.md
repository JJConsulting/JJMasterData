# HtmlBuilder (`JJConsulting.Html`)

`HtmlBuilder` creates HTML programmatically through a fluent API. It is the type returned by many JJMasterData UI components and is useful when an event handler, component, or Tag Helper needs to compose or change markup without building strings manually.

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

## Text, encoding, and raw markup

`AppendText` HTML-encodes text. Attribute values are also encoded when the builder is rendered. This is the appropriate default for values that can originate outside trusted application code.

```csharp
var html = HtmlBuilder.P().AppendText("<Admin>");
// <p>&lt;Admin&gt;</p>
```

`new HtmlBuilder(string)` also encodes its text by default. To deliberately add pre-rendered markup, pass `encode: false` and append that fragment:

```csharp
var icon = new HtmlBuilder("<i class=\"fa fa-user\"></i>", encode: false);
var html = HtmlBuilder.Div().Append(icon);
```

Only use `encode: false`, `AppendStyle`, and `AppendScript` with content fully controlled by the application. They write their contents without HTML encoding and must never receive untrusted user input.

## Attributes and conditional output

Use `WithAttribute` for any HTML attribute. Convenience methods cover common attributes: `WithId`, `WithName`, `WithNameAndId`, `WithValue`, `WithHref`, `WithStyle`, `WithOnClick`, and `WithOnChange`.

```csharp
var input = HtmlBuilder.Input()
    .WithNameAndId("email")
    .WithAttribute("type", "email")
    .WithValue("customer@example.com")
    .WithCssClass("form-control");
```

`WithCssClass` merges classes case-insensitively, avoiding duplicates. Use `WithCssClassIf`, `WithAttributeIf`, and `WithAttributeIfNotEmpty` when an attribute depends on state. `AppendIf` and `AppendTextIf` provide the matching conditional composition operations.

```csharp
var saveButton = HtmlBuilder.Button()
    .WithAttribute("type", "submit")
    .WithCssClass("btn btn-primary")
    .WithCssClassIf(isBusy, "disabled")
    .WithAttributeIf(isBusy, "disabled")
    .AppendTextIf(!isBusy, "Save")
    .AppendTextIf(isBusy, "Saving...");
```

An attribute whose value is `null` is rendered as a boolean attribute (for example, `disabled`). Attributes are stored by name case-insensitively; setting the same name again replaces its value. `GetAttribute` and `TryGetAttribute` can inspect attributes previously added to a tagged builder.

## Razor and Tag Helpers

`HtmlBuilder` implements ASP.NET Core's `IHtmlContent`. Razor and Tag Helpers therefore render it directly; do not convert it to a string only to set it back as HTML content.

```csharp
public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
{
    var childContent = await output.GetChildContentAsync();

    var panel = HtmlBuilder.Div()
        .WithCssClass("panel")
        .Append(childContent);

    output.Content.SetHtmlContent(panel);
}
```

In a JJMasterData grid event, modify `args.HtmlBuilder` or assign a builder to the applicable HTML result argument. See [Grid events](grid-events.md) for the rendering hooks and their scope.

## Scripts and styles

`AppendScript` wraps a raw JavaScript fragment in `<script type="text/javascript">`; `AppendStyle` wraps raw CSS in `<style>`. Their `If` variants append only when the condition is true.

```csharp
var fragment = HtmlBuilder.Div()
    .AppendText("Ready")
    .AppendScriptIf(enableDiagnostics, "console.debug('fragment rendered');");
```

Prefer static bundled assets for shared code. These methods are suited to small, trusted fragments that must accompany dynamically generated HTML.
