# Data panels and application-owned saves

`JJDataPanel` renders dictionary fields inside an MVC page. The controller loads, validates and saves the record; the view renders the panel and the submit button.

## Controller

This example edits an existing `OrderHeader` dictionary with `Id` (primary key), `Title` (required text) and `Notes` (optional text). Use the [configured MVC host](../getting-started/configuration.md), its existing authentication and controller routing.

```csharp
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.UI.Components;
using JJMasterData.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SampleApp.Controllers;

[Authorize]
[AutoValidateAntiforgeryToken]
public sealed class OrdersController(
    IFormElementComponentFactory<JJDataPanel> panelFactory,
    IEntityRepository entities) : Controller
{
    [AcceptVerbs("GET", "POST")]
    public async Task<IActionResult> Edit(int id)
    {
        var panel = await panelFactory.CreateAsync("OrderHeader");
        panel.Name = "order-header-editor";
        panel.PageState = PageState.Update;
        await panel.LoadValuesFromPkAsync(new Dictionary<string, object> { ["Id"] = id });

        if (panel.Values.Count == 0)
            return NotFound();

        // Apply the application's order-specific authorization here.
        panel.SecretValues = new Dictionary<string, object?> { ["Id"] = id };
        panel.FormElement.Fields["Id"].EnableExpression = "val:0";

        if (Request.HasFormContentType && Request.Form["intent"] == "save")
        {
            var values = await panel.GetFormValuesAsync();
            panel.Errors = panel.ValidateFields(values, PageState.Update);

            if (panel.Errors.Count == 0)
            {
                await entities.UpdateAsync(panel.FormElement, values);
                return RedirectToAction(nameof(Edit), new { id });
            }
        }

        var result = await panel.GetResultAsync();
        if (result is IActionResult actionResult)
            return actionResult;

        ViewData["PanelHtml"] = result.HtmlContent;
        ViewData["OrderId"] = id;
        return View();
    }
}
```

GET loads the record. Clicking **Save** reads the posted values, validates them and updates the record. Validation errors stay in `panel.Errors` and the same view retains the entered values. A successful save redirects to GET.

The action also receives field reloads and lookup callbacks. Those POSTs have no save-button intent, so they render the component without writing. Return component `IActionResult` responses directly so callbacks receive their expected response.

## MVC view

Ensure `Views/_ViewImports.cshtml` registers the Tag Helpers:

```cshtml
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@addTagHelper *, JJMasterData.Web
```

In `Views/Orders/Edit.cshtml`:

```cshtml
<master-data-form asp-controller="Orders"
                  asp-action="Edit"
                  asp-route-id="@(ViewData["OrderId"])"
                  asp-antiforgery="true"
                  method="post">
    @ViewData["PanelHtml"]

    <button type="submit" name="intent" value="save" class="btn btn-primary">
        Save
    </button>
</master-data-form>
```

Use the existing layout with `_MasterDataStylesheets` and `_MasterDataScripts` loaded before the panel HTML, or use the built-in `_MasterDataLayout`. `result.HtmlContent` is `IHtmlContent`, so Razor renders it through `ViewData` without `Html.Raw`.

The Tag Helper supplies the form ID expected by MasterData's scripts and the antiforgery token. Keep `intent=save` on the button, not a hidden input, so field callbacks do not save. Do not nest this form inside another form.

## Persistence behavior

This example explicitly validates fields/rules and calls `IEntityRepository.UpdateAsync`. It does not run form-event handlers, form-service text transformations or normal form audit processing. Use `JJFormView.UpdateFormValuesAsync` when the full form workflow is required.

For insertion, use `PageState.Insert`, skip the record lookup, validate in insert state and call `InsertAsync`. Redirect using the generated key in the values dictionary.

## Read and protect values

| Member | Purpose |
| --- | --- |
| `Values` | Loaded or initial field values. |
| `UserValues` | Additional expression context; not automatically persisted as fields. |
| `SecretValues` | Protected values merged over processed field values. |
| `GetFormValuesAsync()` | Merge request values, defaults/triggers and non-null secret values into the panel's values. |
| `PageState` | State used when rendering and processing fields. |

`SecretValues` is serialized with Data Protection into a hidden input when rendering. On submission the panel can recover that protected dictionary. It is intended for values the browser must not edit, such as a parent identifier supplied by the host. It is not a general secret vault, a freshness check or an authorization policy. Rebuild sensitive context from an authorized server lookup on each request when possible.

Null entries in `SecretValues` are removed before merging, so a null entry does not force a field to SQL null. `GetFormValuesAsync()` also removes null entries from `Values`. If omission and explicit clearing have different meanings in your command, model that distinction in its request contract.

See [Create a task-specific form](../cookbook/task-form.md) for protected values and a save through the form workflow.

## Choose the save pipeline

| Approach | What the application must account for |
| --- | --- |
| `JJFormView.InsertFormValuesAsync` / `UpdateFormValuesAsync` | Uses the form-service workflow; handlers are attached when the form is created by name. |
| Standalone `JJDataPanel` and application save service | The application owns validation, persistence, transaction and follow-up behavior. |
| `panel.ValidateFields(values, pageState)` then repository write | Field/rule validation is separate from the write; form handlers and form audit processing do not run automatically. |

`DataPanelFactory.CreateAsync(name)` loads metadata but does not attach `IFormEventHandler` or call `OnFormElementLoadAsync`. The form-view factory does. Do not assume the two factories have the same event lifecycle. Similarly, a grid created independently does not acquire form handlers.

`ValidateFields(values, pageState)` allows an explicit validation state. In the current implementation, the panel's `ValidateFieldsAsync(values)` uses `PageState.Delete` internally; it is not an asynchronous equivalent of validating insert/update state. For an insert/update that needs the normal workflow, use the form-view methods instead.

Rendering a panel and reading its values never saves a record. `GetResultAsync()` can return a component response that needs MVC handling; preserve action results as described in [Routes and embedding](routes-and-embedding.md).

## Compose a custom page

Assign distinct component names and include the MasterData asset partials and form Tag Helper in the host. Recreate the same dictionaries, names and context on subsequent POST/AJAX requests. For a header and several detail lists that must commit together, see [Save a composed document](../cookbook/composed-document.md).
