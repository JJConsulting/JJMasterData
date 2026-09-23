# Data dictionary model

`FormElement` extends the Commons `Element` model with rendering, validation and API settings.

| Member | What it controls |
| --- | --- |
| `Name` | Dictionary identifier used in routes, repository lookups and event registration. |
| `TableName`, `Schema`, `ConnectionId` | Database target; a null connection ID selects the default connection. |
| `Fields` | `FormElementField` values: database type, key, editor, expressions and layout. |
| `Panels` | Field groups; each field refers to a panel through `PanelId`. |
| `Relationships`, `Indexes` | Related entities and database index metadata. |
| `Options` | Grid/form layout, action collections and audit logging. |
| `Rules` | SQL or server-side JavaScript validation before writes. |
| `ApiOptions` | Which data API operations are enabled. |

`DataType`, `Size`, `IsPk`, `AutoNum` and `IsRequired` must match the database contract. Changing the visual `Component` does not change the SQL column type.

Use database scripts to apply schema changes. Saving a `FormElement` only persists the dictionary definition.
