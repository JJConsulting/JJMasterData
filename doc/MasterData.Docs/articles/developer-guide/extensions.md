# Extension points

Choose an extension according to the behavior being changed:

| Requirement | Extension | Guide |
| --- | --- | --- |
| Validate or modify submitted values; react after persistence | `IFormEventHandler` | [Form events](form-events.md) |
| Customize grid filters, data and rendered content | `IGridEventHandler` | [Grid events](grid-events.md) |
| Add a downloadable file format and its options | `IExportFormat<TOptions>` | [Custom export formats](export-formats.md) |
| Add an expression provider | `WithExpressionProvider<T>()` | [Expressions](../concepts/expressions.md) |
| Add an action plugin | `WithActionPlugin<TPlugin>()` | [Actions](../concepts/actions.md) |

Register form and grid handlers through `builder.Services`, keyed by dictionary name. Register export formats, expression providers and action plugins through the builder returned by `AddJJMasterDataWeb()` or `AddJJMasterDataCore()`.

For declarative business validation, start with [dictionary rules](../concepts/rules.md). For metadata and layout changes on a particular form instance, see [UI customization](ui-customization.md).
