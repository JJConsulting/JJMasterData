using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using CsvHelper;
using CsvHelper.Configuration;

namespace JJMasterData.Core.DataManager.Importation;

internal sealed class CsvImportationReader(TextReader reader, CultureInfo culture, char separator, bool detectDelimiter)
{
    private readonly CsvConfiguration _configuration = new(culture)
    {
        HasHeaderRecord = false,
        IgnoreBlankLines = false,
        Delimiter = separator.ToString(),
        DetectDelimiter = detectDelimiter,
        DetectDelimiterValues = [",", ";", "|", "\t"]
    };

    public async IAsyncEnumerable<string[]> ReadRecordsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var csv = new CsvReader(reader, _configuration);

        while (await csv.ReadAsync())
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return csv.Parser.Record ?? [];
        }
    }
}
