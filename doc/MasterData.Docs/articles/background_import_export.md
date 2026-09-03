# Background import and export

Import and export operations use an in-memory queue consumed by a .NET `BackgroundService` by default. Each submission returns a unique job identifier; status and cancellation calls must include both that identifier and the current user. Queued jobs and status records do not survive an application restart. Generated files remain in the configured `IFileStorage`.

Configure capacity, concurrency, and completed-job retention under `JJMasterData:BackgroundJobs`:

```json
{
  "JJMasterData": {
    "BackgroundJobs": {
      "Capacity": 1000,
      "MaxConcurrency": 100,
      "CompletedJobRetention": "01:00:00"
    }
  }
}
```

## Adding an export format

Export formats are regular scoped services. Their options type supplies the stable format id, file metadata, defaults, and the properties used to generate the export modal. The format only serializes the supplied columns and asynchronous rows; data retrieval, paging, progress, file naming, and storage belong to the export pipeline.

```csharp
public sealed class JsonExportOptions : ExportFormatOptions
{
    protected override string Id => "json";
    protected override string DisplayName => "JSON";
    protected override string FileExtension => "json";
    protected override string ContentType => "application/json";

    [Display(Name = "Indented")]
    public bool Indented { get; set; }
}

public sealed class JsonExportFormat : IExportFormat<JsonExportOptions>
{
    public async Task WriteAsync(
        ExportContext context,
        JsonExportOptions options,
        Stream output,
        CancellationToken cancellationToken)
    {
        var rows = new List<IReadOnlyDictionary<string, object?>>();
        await foreach (var row in context.Rows.WithCancellation(cancellationToken))
            rows.Add(row.Values);

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
builder.AddExportFormat<JsonExportFormat, JsonExportOptions>();
```

Every public writable option is included in the UI. Use `DisplayAttribute.Name` to customize its label. Boolean
properties render as Yes/No fields, while enum properties render as selects. On enum members, `Name` is the visible
choice label and `ShortName` is the value posted by the form.

Import readers follow the equivalent `IImportReader<TOptions>` and `AddImportReader<TReader, TOptions>()` contracts. Readers turn a stream into normalized `ImportRecord` instances; validation, expressions, persistence, events, reporting, and cleanup remain in the shared import pipeline.
