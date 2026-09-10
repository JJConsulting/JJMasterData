# Import

Import uploads a CSV file through the import action, parses it, and applies each row to the form as an insert,
update, or delete. The file is saved to the configured `IFileStorage`, processed by a background job, and deleted
when the job finishes. See [Background Jobs](background.md) for the job pipeline itself.

## Supported files

Imports accept CSV content with one record per line. The delimiter can be fixed or detected automatically from the
first lines:

| Delimiter | Enum value |
| --- | --- |
| Semicolon (`;`) | `CsvImportDelimiter.Semicolon` |
| Comma (`,`) | `CsvImportDelimiter.Comma` |
| Pipe (`\|`) | `CsvImportDelimiter.Pipe` |
| Tab | `CsvImportDelimiter.Tab` |

The parser supports byte-order marks, quoted fields, escaped quotes (`""`), and multi-line values inside quotes.
Blank rows are ignored. Delimiters are configured through `CsvImportOptions`:

```csharp
var options = new CsvImportOptions
{
    Delimiter = CsvImportDelimiter.Semicolon,
    DetectDelimiter = true
};
```

## Column mapping

Values are mapped by position to the form fields that are visible during import (`VisibleExpression`) and whose
`DataBehavior` is `Real` or `WriteOnly`. Each record must contain exactly one value per field; rows with a different
column count are reported as errors. If the first record matches the label of the first field, it is treated as a
header row and skipped.

## Row processing

For each record the pipeline:

1. Parses values according to each field's data type and component;
2. Merges relation values and values produced by field expressions;
3. Validates the record;
4. Executes `InsertOrReplaceAsync`, deciding between insert, update, or delete based on the record's key;
5. Raises form events (`OnBeforeImportAsync`, `OnAfterInsertAsync`, `OnAfterUpdateAsync`, `OnAfterDeleteAsync`) and
   runs the optional SQL commands configured before and after the process.

The job reports progress with counters for inserted, updated, deleted, ignored, and failed rows, listing the errors
found per row.
