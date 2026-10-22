# Grid templates

A field's `GridRenderingTemplate` customizes the value rendered in its grid cell. The template receives the values of the current record, so it can combine fields even when the template is configured on only one grid column. HTML templates use Liquid through Fluid:

```liquid
<div class="fw-semibold">{{ FirstName }} {{ LastName }}</div>
<small class="text-muted">{{ Email }} · {{ Phone }}</small>
```

For example, configure this template on the `FirstName` field to show a contact summary instead of only the first name. The other fields (`LastName`, `Email` and `Phone`) must also be present in the dictionary/grid data.

## Quick Liquid reference

Print a value with `{{ ... }}`. Use `{% ... %}` for control flow; whitespace and HTML outside these expressions are rendered as-is.

```liquid
<div class="fw-semibold">{{ Name }}</div>
{% if IsActive %}
  <span class="badge text-bg-success">Active</span>
{% else %}
  <span class="badge text-bg-secondary">Inactive</span>
{% endif %}
```

Loop over a collection with `for` and use `assign` for a reusable value:

```liquid
{% assign displayName = FirstName | capitalize %}
<span title="{{ Email }}">{{ displayName }} {{ LastName }}</span>
```

Guard optional values before rendering them:

```liquid
{% if isNullOrWhiteSpace(Description) %}
  <em>No description</em>
{% else %}
  {{ Description }}
{% endif %}
```

The available helpers are functions, so call them with parentheses, for example `formatDate(CreatedAt, "yyyy-MM-dd")` and `getFileUrl(FileName)`. Use the `|` syntax for standard Liquid filters such as `capitalize` when appropriate.

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

`getFileUrl` needs `DataFile` and record values; it returns an empty string outside that context. Handle empty values explicitly rather than assuming every row has a value.

For Razor views, register `@addTagHelper *, JJMasterData.Web` in `_ViewImports.cshtml`. Include `_MasterDataStylesheets` and `_MasterDataScripts` in custom layouts so generated controls have their required assets.
