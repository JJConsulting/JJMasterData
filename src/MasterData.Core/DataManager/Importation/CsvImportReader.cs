using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using JJMasterData.Core.DataManager.Importation.Abstractions;

namespace JJMasterData.Core.DataManager.Importation;

public sealed class CsvImportReader : IImportReader<CsvImportOptions>
{
    public async IAsyncEnumerable<ImportRecord> ReadAsync(
        ImportContext context,
        CsvImportOptions options,
        Stream input,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var textReader = new StreamReader(input, Encoding.UTF8, true, leaveOpen: true);
        var delimiter = options.Delimiter switch
        {
            CsvImportDelimiter.Semicolon => ';',
            CsvImportDelimiter.Comma => ',',
            CsvImportDelimiter.Pipe => '|',
            CsvImportDelimiter.Tab => '\t',
            _ => throw new InvalidDataException($"Unsupported CSV delimiter '{options.Delimiter}'.")
        };
        var csvReader = new CsvImportationReader(
            textReader,
            CultureInfo.CurrentCulture,
            delimiter,
            options.DetectDelimiter);
        long rowNumber = 0;
        await foreach (var values in csvReader.ReadRecordsAsync(cancellationToken))
            yield return new ImportRecord(++rowNumber, [.. values]);
    }
}
