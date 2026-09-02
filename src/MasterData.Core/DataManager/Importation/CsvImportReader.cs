using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using JJMasterData.Core.DataManager.Exportation;
using JJMasterData.Core.DataManager.Importation.Abstractions;

namespace JJMasterData.Core.DataManager.Importation;

public sealed class CsvImportOptions
{
    public string Delimiter { get; set; } = ";";
    public bool DetectDelimiter { get; set; } = true;
}

public sealed class CsvImportReader : IImportReader<CsvImportOptions>
{
    public ImportFormatDefinition Definition { get; } = new()
    {
        Id = "csv",
        DisplayName = "CSV",
        FileExtensions = [".csv", ".txt", ".tsv"],
        ContentTypes =
            ["text/csv", "text/plain", "text/tab-separated-values", "application/csv", "application/vnd.ms-excel"],
        Options =
        [
            new ExportFormatOption
            {
                Name = nameof(CsvImportOptions.Delimiter),
                DisplayName = "Delimiter",
                Kind = ExportFormatOptionKind.Select,
                DefaultValue = ";",
                Choices =
                    [new(";", "Semicolon (;)"), new(",", "Comma (,)"), new("|", "Pipe (|)"), new("\\t", "Tab")]
            },
            new ExportFormatOption
            {
                Name = nameof(CsvImportOptions.DetectDelimiter),
                DisplayName = "Detect delimiter",
                Kind = ExportFormatOptionKind.Boolean,
                DefaultValue = "true"
            }
        ]
    };

    public async IAsyncEnumerable<ImportRecord> ReadAsync(
        ImportContext context,
        CsvImportOptions options,
        Stream input,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var textReader = new StreamReader(input, Encoding.UTF8, true, leaveOpen: true);
        var delimiter = options.Delimiter.Replace("\\t", "\t");
        var csvReader = new CsvImportationReader(
            textReader,
            CultureInfo.CurrentCulture,
            string.IsNullOrEmpty(delimiter) ? ';' : delimiter[0],
            options.DetectDelimiter);
        long rowNumber = 0;
        await foreach (var values in csvReader.ReadRecordsAsync(cancellationToken))
            yield return new ImportRecord(++rowNumber, [.. values]);
    }
}
