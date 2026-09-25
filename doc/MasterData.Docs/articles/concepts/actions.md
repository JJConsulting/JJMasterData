# Actions

Actions are commands triggered from a grid toolbar, a record row, the form toolbar or a field. Their location determines whether they operate on the list, selected records or the open record.

Use built-in actions for CRUD operations, filtering and import/export. For application-specific commands, choose the type by execution target:

| Requirement | Action |
| --- | --- |
| Open another dictionary | `InternalAction`. |
| Navigate to an application URL | `UrlRedirectAction`. |
| Run browser code | `ScriptAction`. |
| Execute a database command | `SqlCommandAction`. |
| Display Liquid/HTML content | `HtmlTemplateAction`. |
| Invoke a registered extension | `PluginAction`. |

Give each action a unique `Name`. Use `VisibleExpression` to hide it, `EnableExpression` to disable it and `ConfirmationMessage` to request confirmation before execution. These UI settings do not replace the host's authorization policy.

A `ScriptAction` runs in the browser; it is not a server-side validation hook. Use a [rule](rules.md) or [form event](../developer-guide/form-events.md) to reject a write.

## Configuration reference

Add actions to the collection matching their scope:

| Collection | Context |
| --- | --- |
| `Options.GridToolbarActions` | Whole grid or selected records. |
| `Options.GridTableActions` | One record in the grid. |
| `Options.FormToolbarActions` | Open form. |
| `Fields["Name"].Actions` | An editor that supports field actions. |

Built-in actions cover insert, edit, view, save, cancel, delete, filter, sort, refresh, import and export. Custom action types include:

| Type | Relevant settings |
| --- | --- |
| `SqlCommandAction` | `SqlCommand`, `IsSubmit`, `ApplyOnSelected`, `RedirectUrl`. |
| `InternalAction` | `ElementRedirect` configures navigation to another dictionary. |
| `UrlRedirectAction` | Navigate to a URL. |
| `ScriptAction` | `OnClientClick` runs JavaScript in the browser. |
| `HtmlTemplateAction` | Render Liquid/HTML content. |
| `PluginAction` | Execute a registered `IPluginHandler`. |

Shared settings include `Text`, `Icon`, `Order`, `ConfirmationMessage`, `VisibleExpression` and `EnableExpression`. `SetVisible(false)` and `SetEnabled(false)` set the corresponding expression to `val:0`.

`SqlCommandAction.ApplyOnSelected` applies the command to each selected row in a toolbar context. Use a row action when the command requires exactly one record.
