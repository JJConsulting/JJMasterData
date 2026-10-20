# Cookbook

These recipes combine dictionary settings, application services and database customization to solve common application requirements. Examples use fictional entities and target the current ASP.NET Core API. SQL examples explicitly target SQL Server.

Start with a working host from [Installation and configuration](../getting-started/configuration.md). Each recipe lists its own metadata and application requirements. Application-specific interfaces in examples are contracts you implement and register in your host.

| Scenario | Recipe |
| --- | --- |
| Show joined descriptions and calculated values without storing them in the base table | [Customize a read procedure](read-procedure.md) |
| Apply database rules while keeping generated CRUD and identity handling | [Customize a write procedure](write-procedure.md) |
| Save a multiple-choice field into a relationship table | [Persist multiple selections](multiple-selections.md) |
| Update several rows without opening each record | [Edit values in a grid](editable-grid.md) |
| Check overlapping dates and prevent deletion of referenced records | [Validate business operations](business-validation.md) |
| Apply a database transition to selected rows | [Run a bulk action](bulk-action.md) |
| Create a focused form with server-defined account and status values | [Create a task-specific form](task-form.md) |
| Save a header and multiple editable detail lists as one document | [Save a composed document](composed-document.md) |
| Reuse calculations and request-aware UI expressions | [Add application expression functions](custom-expression-functions.md) |
| Display a status legend from multiple SQL results | [Show a status guide with HTML templates](html-templates.md) |

For reusable contracts rather than a complete scenario, see [Stored procedures](../developer-guide/stored-procedures.md), [Form events](../developer-guide/form-events.md) and [Grid events](../developer-guide/grid-events.md).
