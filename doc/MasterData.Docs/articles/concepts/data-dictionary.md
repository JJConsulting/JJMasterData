# Data dictionary model

`FormElement` extends the JJMasterData.Commons `Element` model with rendering, validation and API settings.

| Member | What it controls |
| --- | --- |
| `Name` | Dictionary identifier used in routes, repository lookups and event registration. |
| `TableName`, `Schema`, `ConnectionId` | Database target; a null connection ID selects the default connection. |
| `UseReadProcedure`, `ReadProcedureName`, `UseWriteProcedure`, `WriteProcedureName` | Route database operations through stored procedures. |
| `Fields` | `FormElementField` values: database type, key, editor, expressions and layout. |
| `Panels` | Field groups; each field refers to a panel through `PanelId`. |
| `Relationships`, `Indexes` | Related entities and database index metadata. |
| `Options` | Grid/form layout, action collections and audit logging. |
| `Rules` | SQL or server-side JavaScript validation before writes. |
| `ApiOptions` | Which data API operations are enabled. |

`DataType`, `Size`, `IsPk`, `AutoNum` and `IsRequired` must match the database contract. Changing the visual `Component` does not change the SQL column type.

Use database scripts to apply schema changes. Saving a `FormElement` only persists the dictionary definition.

## Field data behavior

`DataBehavior` determines which database commands receive a field. It is independent from the field's component, visibility and enabled state.

| `FieldBehavior` | Read result / generated read projection | Write parameter | Typical use |
| --- | --- | --- | --- |
| `Real` | Yes | Yes | A persisted column, or a value translated by custom procedures. |
| `ViewOnly` | Yes | No | A joined description or calculated result. |
| `WriteOnly` | No | Yes | Input consumed only by a custom write procedure. |
| `Virtual` | No | No | A UI-only control or contextual value handled by application code. |

A disabled or hidden `Real` field still participates in persistence. Use `EnableExpression` to control editing; do not switch persistence behavior simply to make an editor read-only. Generated read SQL initially uses `NULL` placeholders for `ViewOnly` fields, so a joined value needs a customized query.

For SQL Server procedure calls, the read command derives filter parameters from the dictionary's fields and primary keys. Keep filter metadata aligned with the deployed procedure signature, including any nonphysical fields. See [Stored procedures](../developer-guide/repositories/stored-procedures.md) for the exact contract and [Persist multiple selections](../cookbook/multiple-selections.md) for a nonphysical field used by both read and write procedures.
