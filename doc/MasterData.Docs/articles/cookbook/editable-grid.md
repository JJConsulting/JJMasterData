# Edit values in a grid

Users maintaining conversion factors should be able to change several rows and save from the list. Prefer the built-in grid edit/save actions when each row should follow the regular update workflow.

## Enable the built-in workflow

With `form` created by `IFormElementComponentFactory<JJFormView>`:

```csharp
form.FormElement.Options.GridToolbarActions.GridEditAction.SetVisible(true);
form.GridView.CurrentSettings.RecordsPerPage = 25;
```

The built-in action is named `grid-edit` and is hidden by default. Activating it enables grid edit mode and shows the built-in save/cancel actions. The component suppresses paging, sorting and filtering controls during this mode. Keep the page size bounded and filter the data before editing.

Use each field's `EnableExpression` to limit editing. For example:

```csharp
form.FormElement.Fields["ProductId"].EnableExpression = "val:0";
form.FormElement.Fields["UnitCode"].EnableExpression = "val:0";
```

Set calculated fields to `ViewOnly` when they should also be excluded from write commands. `ViewOnly` changes persistence behavior, so do not use it merely to disable an otherwise writable field for one screen. See [field data behavior](../concepts/data-dictionary.md#field-data-behavior).

The built-in grid save reads posted values merged with loaded rows, validates grid fields and invokes the form update workflow for each row. Put business validation in `OnBeforeUpdateAsync`. A custom import path still needs its own `OnBeforeImportAsync` validation.

## When the host owns the save operation

For a dedicated application command, `EnableEditMode` exposes editors and `GetGridValuesAsync` extracts their values:

```csharp
form.GridView.EnableEditMode = true;

// Inside the host's explicit POST save branch, before rendering the result:
var rows = await form.GridView.GetGridValuesAsync();
if (rows is not null)
{
    // Validate, authorize and persist through your application service.
    // Merely reading these dictionaries does not save them.
}
```

This fragment intentionally does not define routing or a save service. The host supplies a POST action, antiforgery handling and a service that implements the operation. Do not process writes on every render. `GetGridValuesAsync` is not the set of all matching records across all pages, nor a list containing only changed rows. Keep database keys authoritative and accept only fields that the command permits.

## Decide transaction behavior

The built-in save processes rows individually and can leave earlier rows saved when a later row fails. It is not one transaction for the whole grid. If every row must succeed or fail together, implement an explicit batch service with validation and a transaction covering the complete batch.

Avoid replacing this with a loop of `IEntityRepository.UpdateAsync` calls unless bypassing form rules, events and auditing is intentional.

## Check the result

Edit two rows and verify persisted values and update hooks. Enter an invalid value and inspect the grid's error presentation. Test a later-row failure and confirm that the application's expected partial-success behavior matches the result. Also verify a filtered list and a list larger than one page so users understand which rows the save affects.
