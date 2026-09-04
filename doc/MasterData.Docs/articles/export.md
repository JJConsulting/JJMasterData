# Export

Export operations run as background jobs. When the user triggers the export action, a modal lists every registered
format with its options; the selected format is queued and the generated file is stored in the configured
`IFileStorage`. See [Background Jobs](background.md) for the job pipeline itself.

## Built-in formats

`AddJJMasterDataCore()` registers these formats by default:

| Format | Id | Options |
| --- | --- | --- |
| CSV | `csv` | Delimiter (`;` `,` `\|`) |
| Text | `txt` | Delimiter (`\t` `;` `,`) |
| Excel (.xls) | `excel` | Show borders, show striped rows |
| Excel (.xlsx) | `xlsx` | Show table style |

All formats inherit `IncludeFirstRowAsHeader` from `ExportFormatOptions`.

## Adding a custom format

A format implements `IExportFormat<TOptions>`: it declares its id, display name, and file extension, and serializes
the supplied columns and asynchronous rows into the output stream. Data retrieval, paging, progress reporting, file
naming, and storage belong to the export pipeline, not to the format.

```csharp
public sealed class JsonExportOptions : ExportFormatOptions
{
    [Display(Name = "Indented")]
    public bool Indented { get; set; }
}

public sealed class JsonExportFormat : IExportFormat<JsonExportOptions>
{
    public string Id => "json";
    public string DisplayName => "JSON";
    public string FileExtension => "json";

    public async Task WriteAsync(
        ExportContext context,
        JsonExportOptions options,
        Stream output,
        CancellationToken cancellationToken)
    {
        var rows = new List<Dictionary<string, object?>>();
        await foreach (var row in context.Rows.WithCancellation(cancellationToken))
            rows.Add(row);

        await JsonSerializer.SerializeAsync(
            output,
            rows,
            new JsonSerializerOptions { WriteIndented = options.Indented },
            cancellationToken);
    }
}
```

Registering it is enough for the format to appear in the standard UI:

```csharp
builder.AddExportFormat<JsonExportFormat>();
```

## Option metadata

Every public writable property of the options type becomes a field in the export modal:

- `DisplayAttribute.Name` customizes the field label; the property name is used otherwise.
- `bool` properties render as Yes/No fields.
- `enum` properties render as selects: on each member, `DisplayAttribute.Name` is the visible choice label and
  `DisplayAttribute.ShortName` is the value posted by the form.
- Any other type convertible from `string` renders as a text input; non-convertible types are rejected at startup.

Option values posted by the form are bound back to a fresh options instance case-insensitively before the job runs.
