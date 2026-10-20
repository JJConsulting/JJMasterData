# Expressions

Expressions calculate a field value or decide whether a field/action is visible or enabled. They use a provider prefix such as `val:`, `exp:` or `sql:`.

| Setting | Purpose | Example |
| --- | --- | --- |
| `DefaultValue` | Supply an initial value. | `exp:utcNow()` |
| `TriggerExpression` | Recalculate a field during form processing. | `exp:{Quantity} * {UnitPrice}` |
| `VisibleExpression` | Show or hide an editor/action. | `exp:{IsUpdate}` |
| `EnableExpression` | Enable or disable an editor/action. | `val:0` |

Placeholders such as `{Quantity}` resolve against the current form context. Enable `AutoPostBack` on an input when changing it must reload the form and recalculate dependent fields.

Use `val:` for literal values, `exp:` for NCalc calculations and `sql:` for database lookups. SQL expressions are supported for defaults and triggers; visibility and enablement require a synchronous provider such as `val:` or `exp:`.

An expression returns a value, not a validation message. Use a [rule](rules.md) to reject invalid data before saving.

## Providers, placeholders and functions

| Prefix | Example | Supported settings |
| --- | --- | --- |
| `val:` | `val:1` | Defaults, triggers, visibility and enablement. |
| `exp:` | `exp:utcNow()` | Defaults, triggers, visibility and enablement. |
| `sql:` | `sql:SELECT Name FROM Person WHERE Id = {Id}` | Defaults and triggers only. |

`val:` returns trimmed text after placeholder substitution. `exp:` evaluates NCalc; `sql:` returns a scalar using the dictionary's connection. SQL value placeholders become parameters, not table or column identifiers.

### Runtime values

| Placeholder | Value |
| --- | --- |
| `{PageState}` | Current operation, such as `INSERT`, `UPDATE` or `LIST`. |
| `{IsInsert}`, `{IsUpdate}`, `{IsDelete}`, `{IsView}`, `{IsList}`, `{IsFilter}`, `{IsImport}` | `1` or `0`. |
| `{UserId}` | `IMasterDataUser.Id`. |
| `{FieldName}` | The request's `fieldName` query parameter. |
| `{CurrentCulture}` | Current culture name. |
| `{Amount}` | A field or contextual value named `Amount`. |

Reserved values take precedence. Other names resolve from `UserValues`, form values, session and then claims.

### Built-in functions

| Function | Behavior |
| --- | --- |
| `now()`, `utcNow()` | Local or UTC current time. |
| `empty()` | Empty string. |
| `iif(condition, yes, no)` | Evaluate the selected branch. |
| `len(value)`, `trim(value)` | String length or whitespace removal. |
| `coalesce(...)` | First value that is neither null nor an empty string. |

For example, `exp:coalesce({Amount}, 0)` supplies a fallback. Register custom NCalc functions through `MasterDataCoreOptions.ExpressionContext`, or add a provider with `WithExpressionProvider<T>()`. Visibility/enablement require `ISyncExpressionProvider`; defaults/triggers require `IAsyncExpressionProvider`.

## Defaults, triggers and persistence

A `DefaultValue` supplies an initial/fallback value; a `TriggerExpression` recalculates a value during processing. For a parent-scoped form, a default can prefill a foreign key while a trigger can keep a contextual value synchronized. Neither replaces server-side ownership checks on update or delete.

`VisibleExpression = "val:0"` hides an editor and `EnableExpression = "val:0"` disables editing. These settings do not remove the field from database commands: that is controlled by [field data behavior](data-dictionary.md#field-data-behavior). A hidden `Real` field and a `Virtual` field have different persistence contracts.

See [Preserve context across requests](../developer-guide/routes-and-embedding.md#preserve-context-across-requests) for the distinction between a listing filter and a foreign-key default.

## Register application functions

Extend `MasterDataCoreOptions.ExpressionContext.Functions` with `PostConfigure<MasterDataCoreOptions>` after registering JJMasterData. Add entries to the existing dictionary so built-in functions remain available. Function arguments expose `Evaluate(index)` and the current expression context.

The `exp:` provider supplies a `ServiceProvider` parameter containing its current service provider. A function can resolve a service during evaluation; avoid capturing scoped services or a particular user in globally configured options. Synchronous expressions can be evaluated repeatedly and outside a browser request, so keep them inexpensive and define what happens without `HttpContext`.

[Add application expression functions](../cookbook/custom-expression-functions.md) demonstrates a calculation and a role-dependent visibility function. Role visibility is presentation logic; enforce the actual operation's policy in the host.
