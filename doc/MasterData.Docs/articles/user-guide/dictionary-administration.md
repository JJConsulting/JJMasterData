# Dictionary administration

Open `/DataDictionary` with an account allowed by the host's administration policy. The editor changes the `FormElement` consumed by both the generated UI and data services.

| Change | Where it is represented |
| --- | --- |
| Database mapping | `TableName`, `Schema`, `ConnectionId`, read/write procedure names. |
| Field contract | `DataType`, `Size`, `IsPk`, `AutoNum`, `IsRequired`. |
| Layout and editors | Panels, `Component`, `LineGroup`, `CssClass`. |
| Commands | Grid toolbar, row, form toolbar and field action collections. |
| Validation | SQL/JavaScript rules and their operation flags. |
| HTTP operations | `ApiOptions`. |

Use **Database Scripts** to review table and stored-procedure changes, and **Preview** to exercise the generated form. Saving a dictionary does not execute schema scripts.

After changing a key, type or size, check both existing data and generated write behavior. Export the dictionary JSON for code review and deployment; deploy any required database changes separately.
