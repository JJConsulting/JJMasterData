# Razor, Tag Helpers, and custom helpers

Use [`HtmlBuilder`](index.md) with the `JJConsulting.Html` and `JJConsulting.Html.Extensions` namespaces.

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

In a JJMasterData grid event, modify `args.HtmlBuilder` or assign a builder to the applicable HTML result argument. See [Grid events](../customizations/grid-events.md) for the rendering hooks and their scope.

## Add project-specific helpers

Applications can add extension members for repeated attributes or components while keeping the underlying builder API:

```csharp
public static class AppHtmlBuilderExtensions
{
    extension(HtmlBuilder html)
    {
        public HtmlBuilder WithDataAttribute(string name, string value) =>
            html.WithAttribute($"data-{name}", value);
    }
}
```

This syntax uses C# 14 extension members. Keep helpers that emit raw markup restricted to trusted values, just like the built-in [script and style methods](encoding.md#scripts-and-styles).

[HtmlBuilder overview](index.md)
