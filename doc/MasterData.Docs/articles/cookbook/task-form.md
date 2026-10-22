# Create a task-specific form

A support page starts a service ticket for an account selected on a previous screen. The operator enters a subject and description; the account and initial status come from the server. Reuse a `ServiceTicket` dictionary while tailoring its runtime form to this task.

## Prepare the dictionary and context

Define `Id` (identity primary key), `AccountId`, `Status`, `Subject` and `Description`. The last four are writable fields; configure the required settings in the dictionary. Protect the host's GET and POST actions, authorize the selected account, and check that it still accepts new tickets before creating the form.

After obtaining `authorizedAccountId` from that server-side check:

```csharp
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.UI.Components;

// formFactory is IFormElementComponentFactory<JJFormView>.
var form = await formFactory.CreateAsync("ServiceTicket");
form.DataPanel.PageState = PageState.Insert;
form.DataPanel.SecretValues = new Dictionary<string, object?>
{
    ["AccountId"] = authorizedAccountId,
    ["Status"] = "New"
};
form.FormElement.Fields["AccountId"].VisibleExpression = "val:0";
form.FormElement.Fields["Status"].VisibleExpression = "val:0";
form.FormElement.Fields["Subject"].Label = "What needs attention?";
```

These changes belong to the runtime form. Do not save account-specific values into shared dictionary metadata. Reapply them before processing every POST and AJAX callback. A hidden field alone can still be posted with a different value; the protected values are merged over processed field values by `GetFormValuesAsync()`.

## Save through the form workflow

For a page with its own **Create ticket** POST action, after recreating the same form and authorizing the account:

```csharp
// Execute only in the host's explicit POST save branch.
var values = await form.GetFormValuesAsync();
var errors = await form.InsertFormValuesAsync(values);

if (errors.Count > 0)
{
    form.DataPanel.Errors = errors;
    // Re-render the form with its entered values and errors.
}
else
{
    var ticketId = Convert.ToInt32(values["Id"]);
    // Redirect to the authorized detail page for this ticket.
}
```

The host supplies routing, antiforgery validation and MVC response handling. Use the form Tag Helper and preserve component action results as described in [Routes and embedding](../developer-guide/customizations/routes-and-embedding.md). Choose either the normal built-in save action or your explicit save branch for a submission; do not invoke both.

Creating `JJFormView` by dictionary name attaches registered form handlers. Its insertion method runs the form-service workflow and recovers generated identity values when the provider returns them. A standalone data panel followed by `IEntityRepository.InsertAsync` has a different validation/event/audit lifecycle.

## Keep context current

A protected round-trip value can still refer to an account whose permissions or status changed after the page was rendered. Reauthorize it on POST. Never use an arbitrary submitted account identifier to populate `SecretValues` without a server check. Do not put null into `SecretValues` to request clearing a field; null entries are removed before merging.

Imports and API writes do not recreate this screen's per-request setup. If those entry points are enabled, enforce their account and status rules separately, or place shared checks in the handler/service that they use.

## Check the result

Create a ticket, confirm its account and initial status, and follow the generated key to its detail page. Submit invalid required fields and confirm the entered text survives redisplay. Tamper with posted account/status fields, then change account access after rendering: both attempts must follow the server's current checks.

See [Data panels](../developer-guide/customizations/data-panels.md) for value precedence and factory differences.
