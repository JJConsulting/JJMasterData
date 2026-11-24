using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Exportation;
using JJMasterData.Core.DataManager.Exportation.Formats;

namespace JJMasterData.Core.Test.DataManager.Exports;

public sealed class DelimitedTextExportFormatTests
{
    [Theory]
    [InlineData(";", CsvExportDelimiter.Semicolon)]
    [InlineData(",", CsvExportDelimiter.Comma)]
    [InlineData("|", CsvExportDelimiter.Pipe)]
    [InlineData("\t", null)]
    public async Task WriteAsync_ShouldEscapeInjectionPrefixes_WhenExportingDelimitedText(
        string delimiter, CsvExportDelimiter? csvDelimiter)
    {
        string[] dangerous = ["=1+1", "+1+1", "-1+1", "@SUM(A1)", "\t=1+1", "\r=1+1",
            "=1;\"quoted\",|value", "-42"];
        string?[] unchanged = ["ordinary", "semi;comma,pipe|tab\tquote\"", "line\nbreak", "", null,
            " =1+1", "\n=1+1", "＝1+1", "＋1+1", "－1+1", "＠SUM(A1)"];
        var values = dangerous.Concat(unchanged).ToArray();
        var context = new ExportContext
        {
            FormElement = new FormElement { Name = "export" },
            Columns =
            [
                new FormElementField { Name = "text", Label = "=1+1" },
                new FormElementField { Name = "number", DataType = FieldType.Decimal }
            ],
            Rows = Rows(values),
            UserValues = [],
            TotalRecords = values.Length,
            Progress = new ExportProgressReporter(values.Length, new Progress<ExportProgress>())
        };
        await using var output = new MemoryStream();
        if (csvDelimiter.HasValue)
            await new CsvExportFormat().WriteAsync(context, new CsvExportOptions { Delimiter = csvDelimiter.Value },
                output, TestContext.Current.CancellationToken);
        else
            await new TextExportFormat().WriteAsync(context, new TextExportOptions(),
                output, TestContext.Current.CancellationToken);

        output.Position = 0;
        using var reader = new StreamReader(output);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.CurrentCulture)
        {
            Delimiter = delimiter,
            HasHeaderRecord = false
        });
        Assert.True(await csv.ReadAsync());
        Assert.Equal("'=1+1", csv.GetField(0));
        Assert.Equal("number", csv.GetField(1));
        for (var index = 0; index < values.Length; index++)
        {
            Assert.True(await csv.ReadAsync());
            Assert.Equal(index < dangerous.Length ? "'" + values[index] : values[index] ?? "", csv.GetField(0));
            Assert.Equal("'" + (-12.5m).ToString(CultureInfo.CurrentCulture), csv.GetField(1));
            Assert.Equal(2, csv.Parser.Count);
        }
        Assert.False(await csv.ReadAsync());
    }

    private static async IAsyncEnumerable<Dictionary<string, object?>> Rows(IEnumerable<string?> values)
    {
        foreach (var value in values)
        {
            yield return new Dictionary<string, object?> { ["text"] = value, ["number"] = -12.5m };
            await Task.Yield();
        }
    }
}
