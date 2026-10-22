# Run a bulk action

An operations grid needs a **Close selected** command. Use a toolbar `SqlCommandAction` with selection enabled when the operation belongs in the database and only needs primary keys plus trusted context.

## Configure the runtime grid

Assume a `WorkOrder` dictionary with an `Id` primary key, and a deployed `dbo.CloseWorkOrder` procedure accepting `@Id`. Configure the form before processing its result on every request:

```csharp
using JJMasterData.Core.DataDictionary.Models.Actions;

form.GridView.EnableMultiSelect = true;
form.GridView.ToolbarActions.Add(new SqlCommandAction
{
    Name = "close-selected",
    Text = "Close selected",
    ConfirmationMessage = "Close the selected work orders?",
    ApplyOnSelected = true,
    SqlCommand = "EXEC dbo.CloseWorkOrder @Id = {Id}"
});
```

Placeholders are parsed into SQL parameters. Do not surround `{Id}` with SQL quotes. Commands use the dictionary's `ConnectionId`. Define authorization and allowed status transitions in the application/database execution path; the confirmation message and action visibility only affect the UI.

## Design the command for selected keys

In a selected-row toolbar action, the command context contains primary-key values, not all displayed columns. A status badge visible in the grid does not make `{Status}` available here. Load and check the current status inside `CloseWorkOrder` using `@Id`.

For each record, verify existence, access and transition eligibility at execution time. Use trusted server context for the actor when needed. Decide whether closing an already closed record should be idempotent or an error. Do not trust a previously rendered checkbox as proof of current eligibility.

The built-in SQL action creates one command per selected key and submits the list through `IEntityRepository.SetCommandListAsync`. With the default repository this uses one database transaction for the command list. A failure rolls back database work in that transaction; a replaced repository must preserve the application's expected semantics. Calls to email or external services cannot be rolled back by that database transaction.

SQL actions execute commands directly. They do not invoke the form's before/after hooks or normal form audit processing. If the transition needs an audit record, write it explicitly within the database operation, or use an application command that supplies the required workflow.

## Inspect selection in a custom handler

When the host owns a batch service, retrieve selected keys with:

```csharp
var selectedKeys = form.GridView.GetSelectedGridValues();
foreach (var key in selectedKeys)
{
    var id = Convert.ToInt32(key["Id"]);
    // Load current state and authorize through your batch service.
}
```

The result contains key dictionaries and an internal `INTERNALPK` entry. It is not an editable row snapshot. Use [grid editing](editable-grid.md) when users must submit changed field values instead of selecting an operation.

## Check the result

Try no selection, one eligible row and several eligible rows. Include a row whose state changed after rendering and verify the intended error and rollback behavior. Confirm that keys from a different access scope are rejected server-side and that audit records are written only for committed transitions.
