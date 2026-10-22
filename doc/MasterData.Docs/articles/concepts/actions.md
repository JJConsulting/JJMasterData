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

A `ScriptAction` runs in the browser; it is not a server-side validation hook. Use a [rule](rules.md) or [form event](../developer-guide/customizations/form-events.md) to reject a write.

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

## Selected-row SQL commands

For a grid toolbar `SqlCommandAction`, `ApplyOnSelected` takes effect when the grid also has `EnableMultiSelect` enabled. Each selected row supplies its primary keys to the expression context, not the full displayed record. Look up any current business values inside the command instead of assuming a displayed column is available as a placeholder.

With the default repository, the generated command list executes in one transaction on the dictionary's connection. SQL actions bypass the normal form-validation, form-event and audit pipeline, so any required checks and audit writes belong in the command or an application workflow. See [Run a bulk action](../cookbook/bulk-action.md).

The built-in `GridEditAction` is hidden by default. Enable it in `Options.GridToolbarActions` to expose the grid edit/save/cancel workflow. Saving through that workflow processes updates individually; it does not provide the single transaction used by the selected-row SQL command list. See [Edit values in a grid](../cookbook/editable-grid.md).

## Place custom form-toolbar actions

Set `Location` when adding a custom action to `Options.FormToolbarActions`. The form renders `Top`, `Panel` and `Bottom` groups separately; an action whose nullable `Location` is unset does not belong to any of those rendered groups.

```csharp
using JJMasterData.Core.DataDictionary.Models.Actions;

form.FormElement.Options.FormToolbarActions.Add(new UrlRedirectAction
{
    Name = "copy-document",
    Text = "Copy",
    Location = FormToolbarActionLocation.Bottom,
    UrlRedirect = "/Planning/Copy?sourceId={Id}",
    VisibleExpression = "exp:'{PageState}' = 'VIEW'"
});
```

The host implements and authorizes the target endpoint. A copy operation should create new identities and recheck eligibility when saving, rather than overwriting the source. See [Save a composed document](../cookbook/composed-document.md).
