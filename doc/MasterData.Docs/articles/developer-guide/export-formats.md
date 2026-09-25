# Custom export formats

Implement `IExportFormat<TOptions>` to add a format to the standard export modal. The export pipeline supplies columns and asynchronous rows, binds the selected options, and stores the resulting file. The format writes content to the supplied stream.

## Implement a streaming JSON format

The example writes only fields in `context.Columns`. Rows can contain other repository values, so serializing entire row dictionaries would bypass the export column selection.

```csharp
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using JJMasterData.Core.DataManager.Exportation;
using JJMasterData.Core.DataManager.Exportation.Abstractions;

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
        using var writer = new Utf8JsonWriter(output,
            new JsonWriterOptions { Indented = options.Indented });
        writer.WriteStartArray();
        long processed = 0;

        await foreach (var row in context.Rows.WithCancellation(cancellationToken))
        {
            writer.WriteStartObject();
            foreach (var column in context.Columns)
            {
                writer.WritePropertyName(column.Name);
                row.TryGetValue(column.Name, out var value);
                JsonSerializer.Serialize(writer, value);
            }
            writer.WriteEndObject();
            processed++;
            if (processed % 1000 == 0)
            {
                await writer.FlushAsync(cancellationToken);
                context.Progress.Report(new ExportProgress(
                    processed, context.TotalRecords, $"Exported {processed} records."));
            }
        }

        writer.WriteEndArray();
        await writer.FlushAsync(cancellationToken);
        context.Progress.Report(new ExportProgress(
            processed, context.TotalRecords, $"Exported {processed} records."));
    }
}
```

This example uses the standard implicit usings of the .NET host. It writes field names as JSON keys and leaves typed values to `System.Text.Json`. It does not apply grid HTML, localized display-value formatting or file-link conversion. Implement such transformations explicitly if your format requires them.

`ExportFormatOptions.IncludeFirstRowAsHeader` is inherited and appears in the options metadata. It has no effect in this JSON format, which uses property names instead of a header row; explain this when exposing the format to users.

## Register the format

```csharp
using JJMasterData.Core.Configuration;
using JJMasterData.Web.Configuration;

builder.Services.AddJJMasterDataWeb(builder.Configuration)
    .WithExportFormat<JsonExportFormat>();
```

Add `WithExportFormat` to the application's existing registration chain. The format identifier must be nonempty and unique, ignoring case. The catalog rejects duplicates. Use the extension without a leading dot.

## Export context

| Property | Responsibility |
| --- | --- |
| `FormElement` | Dictionary metadata for this export. |
| `Columns` | Selected export fields, in order, with localized labels. |
| `Rows` | Asynchronous sequence of repository rows. |
| `UserValues` | Values supplied with the export request. |
| `TotalRecords` | Expected count, which can be null. |
| `Progress` | Report `ExportProgress` with processed count, total and message. |

Enumerate rows once, honor cancellation and write incrementally for large exports. Keep the supplied stream open for the pipeline. The pipeline handles repository paging, naming, temporary-file cleanup and saving to `IFileStorage`; formats should not duplicate those steps.

## Option metadata and binding

Options must derive from `ExportFormatOptions` and have a public parameterless constructor. Public writable properties become fields in the modal:

- `DisplayAttribute.Name` supplies the label.
- Boolean properties become Yes/No fields.
- Enum properties become select fields. Member `DisplayAttribute.Name` supplies the label and `ShortName` supplies the posted value.
- Other types must support conversion from string to become text inputs.

The options metadata factory rejects unsupported property types. Submitted names are bound case-insensitively into a fresh options instance; property initializers define defaults. See the built-in format options for examples of delimiters, borders and table styles.

For the user's workflow and built-in formats, see [record import and export](../concepts/crud-operations.md#5-import-and-export-records).
