# Troubleshooting

Use this checklist when investigating a failing operation or a generated UI issue. For Liquid syntax and rendering contexts, see the [Grid templates quick reference](grid-templates.md#quick-liquid-reference) and [TemplateAction quick reference](template-action.md#quick-liquid-reference).

Enable library diagnostics in the host configuration when investigating a failing operation:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "JJMasterData": "Debug"
    }
  }
}
```

| Symptom | Check |
| --- | --- |
| Connection fails | `JJMasterData:ConnectionString`; for an additional connection, match `ConnectionId` to its `Guid`. |
| Missing table or procedure | Dictionary `Schema`, `TableName`, read/write procedure names and generated database scripts. |
| CRUD has no styling or actions fail | Responses for `/_content/JJMasterData.Web/` assets and both MasterData layout partials. |
| Save returns validation errors | `IsRequired`, dictionary rules and before-event `Errors`. |
| Save returns `DbException` | The `FormService` exception log and database permissions/constraints. |
| `getFileUrl` returns empty | File-field context, `DataFile`, file name and record values. |
| Job status disappears after restart | The default background queue is in memory. |

Reproduce with the dictionary name, operation and record key. Templates receiving empty values should handle them explicitly instead of assuming every row has a value.
