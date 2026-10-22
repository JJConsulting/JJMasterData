# Attributes and conditional output

Use [`HtmlBuilder`](index.md) with the `JJConsulting.Html` and `JJConsulting.Html.Extensions` namespaces.

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

`AppendIf` can conditionally append either a builder or a tag. These helpers omit the element or attribute when the condition is false; they do not render a placeholder.

[HtmlBuilder overview](index.md)
