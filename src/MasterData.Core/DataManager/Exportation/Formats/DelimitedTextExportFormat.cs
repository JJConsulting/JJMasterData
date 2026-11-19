using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using JJMasterData.Core.DataManager.Exportation.Abstractions;
using JJMasterData.Core.DataManager.Services;

namespace JJMasterData.Core.DataManager.Exportation.Formats;

internal abstract class DelimitedTextExportFormat<TOptions> : IExportFormat<TOptions> where TOptions : ExportFormatOptions, new()
{
    protected abstract string GetDelimiter(TOptions options);
    protected virtual bool ProtectSpreadsheetCells => false;

    public abstract string Id { get; }
    public abstract string DisplayName { get; }
    public abstract string FileExtension { get; }

    public async Task WriteAsync(
        ExportContext context,
        TOptions options,
        Stream output,
        CancellationToken cancellationToken)
    {
        await using var textWriter = new StreamWriter(output, new UTF8Encoding(true), leaveOpen: true);
        var configuration = new CsvConfiguration(CultureInfo.CurrentCulture)
        {
            Delimiter = GetDelimiter(options),
            HasHeaderRecord = false
        };
        if (ProtectSpreadsheetCells)
        {
            configuration.InjectionOptions = InjectionOptions.Escape;
            configuration.InjectionCharacters = ['=', '+', '-', '@', '\t', '\r', '\n', '＝', '＋', '－', '＠'];
        }

        await using var csv = ProtectSpreadsheetCells
            ? new SpreadsheetCsvWriter(textWriter, configuration)
            : new CsvWriter(textWriter, configuration);

        if (options.IncludeFirstRowAsHeader)
        {
            foreach (var field in context.Columns)
                csv.WriteField(field.LabelOrName);
            await csv.NextRecordAsync();
        }

        long processed = 0;
        await foreach (var row in context.Rows.WithCancellation(cancellationToken))
        {
            foreach (var field in context.Columns)
            {
                row.TryGetValue(field.Name, out var rawValue);
                var value = FieldFormattingService.FormatValue(field, rawValue);
                csv.WriteConvertedField(value, rawValue?.GetType() ?? typeof(string));
            }
            await csv.NextRecordAsync();
            processed++;
            context.Progress.Report(processed);
        }

        context.Progress.Complete(processed);
        await textWriter.FlushAsync(cancellationToken);
    }

    private sealed class SpreadsheetCsvWriter(TextWriter writer, CsvConfiguration configuration)
        : CsvWriter(writer, configuration)
    {
        private bool _isNumber;

        public override void WriteConvertedField(string? field, Type fieldType)
        {
            _isNumber = Type.GetTypeCode(fieldType) is >= TypeCode.SByte and <= TypeCode.Decimal;
            try
            {
                base.WriteConvertedField(field, fieldType);
            }
            finally
            {
                _isNumber = false;
            }
        }

        protected override string? SanitizeForInjection(string? field)
        {
            return _isNumber ? field : base.SanitizeForInjection(field);
        }
    }
}
