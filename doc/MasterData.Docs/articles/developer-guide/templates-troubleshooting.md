# Templates, logging and troubleshooting

HTML templates use Liquid through Fluid. A field's `GridRenderingTemplate` customizes its grid cell; `HtmlTemplateAction` renders action content. See the template reference below for formatting and file links.

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

## Template and UI reference

Record fields are available by name in the rendering context:

```liquid
<strong>{{ Name }}</strong>
<span>{{ formatDate(CreatedAt, "yyyy-MM-dd") }}</span>
```

| Function | Use |
| --- | --- |
| `localize(key, ...)` | Translate with `IStringLocalizer<MasterDataResources>`. |
| `formatDate(value, format)` | Format a parseable date; otherwise return the input text. |
| `dateAsText(value)` | Relative date description. |
| `isNullOrEmpty(value)`, `isNullOrWhiteSpace(value)` | Guard missing text. |
| `trim(value)`, `trimStart(value)`, `trimEnd(value)` | Remove whitespace. |
| `substring(value, start, length)` | Extract text; length is optional. |
| `capitalize(value)` | Convert to title case using the current culture. |
| `urlPath()`, `appUrl()` | Application path or URI. |
| `table(rows)` | Render a Bootstrap table from rows. |
| `getFileUrl(fileName)` | Generate a download URL in a file-field context. |
| `isImage(fileName)` | Recognize `.png`, `.jpg` and `.jpeg`. |

`getFileUrl` needs `DataFile` and record values; it returns an empty string outside that context.

For Razor views, register `@addTagHelper *, JJMasterData.Web` in `_ViewImports.cshtml`. Include `_MasterDataStylesheets` and `_MasterDataScripts` in custom layouts so generated controls have their required assets.
