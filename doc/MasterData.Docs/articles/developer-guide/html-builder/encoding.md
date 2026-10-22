# Text encoding and raw markup

Use [`HtmlBuilder`](index.md) with the `JJConsulting.Html` and `JJConsulting.Html.Extensions` namespaces.

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

`ToString()` renders the complete fragment into a string. When writing to an ASP.NET Core response or another `TextWriter`, prefer `IHtmlContent.WriteTo(writer, encoder)` so the builder can write directly to the destination instead of first creating an intermediate string.

## Scripts and styles

`AppendScript` wraps a raw JavaScript fragment in `<script type="text/javascript">`; `AppendStyle` wraps raw CSS in `<style>`. Their `If` variants append only when the condition is true.

```csharp
var fragment = HtmlBuilder.Div()
    .AppendText("Ready")
    .AppendScriptIf(enableDiagnostics, "console.debug('fragment rendered');");
```

Prefer static bundled assets for shared code. These methods are suited to small, trusted fragments that must accompany dynamically generated HTML.

[HtmlBuilder overview](index.md)
