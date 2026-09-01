using System.Collections.Generic;
using System.IO;
using System.Threading;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Exportation.Abstractions;

namespace JJMasterData.Core.DataManager.Importation.Abstractions;

public sealed class ImportFormatDefinition
{
    public required string Id { get; init; }
    public required string DisplayName { get; init; }
    public required List<string> FileExtensions { get; init; }
    public required List<string> ContentTypes { get; init; }
    public List<ExportFormatOption>? Options { get; init; }
}

public sealed class ImportContext(FormElement formElement, string fileName, string? contentType)
{
    public FormElement FormElement { get; init; } = formElement;
    public string FileName { get; init; } = fileName;
    public string? ContentType { get; init; } = contentType;
}

public sealed class ImportRecord(long rowNumber, List<string?> values)
{
    public long RowNumber { get; init; } = rowNumber;
    public List<string?> Values { get; init; } = values;
}

public interface IImportReader<in TOptions> where TOptions : class, new()
{
    ImportFormatDefinition Definition { get; }
    IAsyncEnumerable<ImportRecord> ReadAsync(
        ImportContext context,
        TOptions options,
        Stream input,
        CancellationToken cancellationToken);
}
