# Save a header and its lines together

Use a `JJDataPanel` for the header and an editable `JJGridView` for the lines. One MVC form submits both; `SetCommandListAsync` saves the changes in one transaction.

This SQL Server example **edits existing rows**. Configure these dictionaries on the same connection, with fields placed in panels:

| Dictionary / table | Fields |
| --- | --- |
| `PlanHeader` / `dbo.PlanHeader` | `PlanId` (primary key), `Title` (required text). |
| `PlanLine` / `dbo.PlanLine` | `LineId` (primary key), `PlanId` (equality filter), `ItemCode` (text), `Quantity` (required decimal with a numeric editor). |

## Controller

The host already registers JJMasterData, authentication and MVC routes. In `PlanningController`, load the components on GET and POST, keeping their names and row keys stable:

```csharp
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.UI.Components;
using JJMasterData.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[AutoValidateAntiforgeryToken]
public sealed class PlanningController(
    IComponentFactory components,
    IEntityRepository entities) : Controller
{
    [AcceptVerbs("GET", "POST")]
    public async Task<IActionResult> Edit(int id)
    {
        var header = await components.DataPanel.CreateAsync("PlanHeader");
        header.Name = "plan-header";
        header.PageState = PageState.Update;
        await header.LoadValuesFromPkAsync(new Dictionary<string, object> { ["PlanId"] = id });
        if (header.Values.Count == 0)
            return NotFound();

        // Apply the application's permission check for this plan here.
        header.SecretValues = new Dictionary<string, object?> { ["PlanId"] = id };
        header.FormElement.Fields["PlanId"].EnableExpression = "val:0";

        var lines = await components.GridView.CreateAsync("PlanLine");
        lines.Name = "plan-lines";
        lines.EnableEditMode = true;
        lines.EnableSorting = false;
        lines.EnableFilter = false;
        lines.ShowPaging = false;
        foreach (var name in new[] { "LineId", "PlanId", "ItemCode" })
            lines.FormElement.Fields[name].EnableExpression = "val:0";

        var storedLines = await entities.GetDictionaryListAsync(lines.FormElement,
            new EntityParameters
            {
                Filters = new() { ["PlanId"] = id },
                OrderBy = new OrderByData().AddOrReplace("LineId", OrderByDirection.Asc),
                RecordsPerPage = 201
            });
        if (storedLines.Count > 200)
            return BadRequest("This editor supports at most 200 lines.");
        lines.DataSource = storedLines;

        if (Request.HasFormContentType && Request.Form["intent"] == "save")
        {
            var values = await header.GetFormValuesAsync();
            var editedLines = (await lines.GetGridValuesAsync())!;
            for (var i = 0; i < editedLines.Count; i++)
            {
                // Keep keys from the loaded rows, not posted editors.
                editedLines[i]["LineId"] = storedLines[i]["LineId"];
                editedLines[i]["PlanId"] = id;
            }

            header.Errors = header.ValidateFields(values, PageState.Update);
            lines.Errors = lines.ValidateGridFields(editedLines);
            if (lines.Errors.Count == 0 &&
                editedLines.Any(row => Convert.ToDecimal(row["Quantity"]) <= 0))
                ModelState.AddModelError("", "Quantities must be greater than zero.");

            if (header.Errors.Count == 0 && lines.Errors.Count == 0 && ModelState.IsValid)
            {
                var commands = new List<DataAccessCommand>
                {
                    (DataAccessCommand)$"UPDATE dbo.PlanHeader SET Title = {values["Title"]} WHERE PlanId = {id}; IF @@ROWCOUNT <> 1 THROW 50001, 'Plan no longer exists.', 1;"
                };
                for (var i = 0; i < editedLines.Count; i++)
                {
                    var lineId = storedLines[i]["LineId"];
                    var quantity = editedLines[i]["Quantity"];
                    commands.Add((DataAccessCommand)$"UPDATE dbo.PlanLine SET Quantity = {quantity} WHERE LineId = {lineId} AND PlanId = {id}; IF @@ROWCOUNT <> 1 THROW 50002, 'A line no longer exists.', 1;");
                }

                await entities.SetCommandListAsync(commands, header.FormElement.ConnectionId);
                return RedirectToAction(nameof(Edit), new { id });
            }

            // Render the attempted quantities when validation fails.
            lines.DataSource = editedLines;
        }

        var result = await header.GetResultAsync();
        if (result is IActionResult actionResult)
            return actionResult;

        ViewData["HeaderHtml"] = result.HtmlContent;
        ViewData["LinesHtml"] = new HtmlString(await lines.GetTableHtmlAsync());
        ViewData["PlanId"] = id;
        return View();
    }
}
```

The explicit cast to `DataAccessCommand` converts interpolated **values into parameters**. Keep table and column names fixed. `SetCommandListAsync` uses the supplied connection and, with the default repository, rolls back all commands if any fails. The row-count checks turn a deleted header/line into a failure instead of silently saving only part of the document.

## MVC view

In `Views/Planning/Edit.cshtml`, use the existing layout and Tag Helper registrations described in [Data panels](../developer-guide/customizations/data-panels.md#mvc-view):

```cshtml
<master-data-form asp-controller="Planning"
                  asp-action="Edit"
                  asp-route-id="@(ViewData["PlanId"])"
                  asp-antiforgery="true"
                  method="post">
    <div asp-validation-summary="All" class="text-danger"></div>

    @ViewData["HeaderHtml"]
    @ViewData["LinesHtml"]

    <button type="submit" name="intent" value="save" class="btn btn-primary">
        Save all
    </button>
</master-data-form>
```

`GetTableHtmlAsync()` returns a string, so wrap that generated markup in `HtmlString` before passing it through `ViewData`. The header's `HtmlContent` already implements `IHtmlContent`. One form submits both sets of editors; no built-in grid save action is needed.

## Details that matter

- **Row identity:** grid input names are derived from primary-key values plus the field name. `GetGridValuesAsync()` needs the loaded rows to find the posted inputs; it does not discover rows from arbitrary form keys. Distinct grid names alone do not prevent input collisions when two grids use the same keys and field names.
- **Validation:** `ValidateGridFields` checks editable, visible fields. The example adds its positive-quantity rule explicitly. Standalone panel/grid factories and direct SQL do not run form-event handlers or normal form audit processing. Put any additional business rules and audit writes in this save operation.
- **Concurrency:** this transaction guarantees atomicity, not stale-edit detection. For shared editing, add an expected-version check inside the transaction and define how concurrent line additions/removals are handled.
- **Adding/removing rows:** this recipe updates the loaded set only. For a draft editor, keep an explicit collection of added/changed/deleted rows, assign unique temporary keys to new rows and map only allowed fields into the save command. Do not interpret a missing posted editor as a request to delete a stored line.

Check a validation failure first, then force a database error on a later line: the entered values should survive validation, and a database failure must leave the header and all lines unchanged.
