using System.Globalization;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Importation;
using JJMasterData.Core.DataManager.Services;

namespace JJMasterData.Core.Test.DataManager.Imports;

public class CsvImportationReaderTests
{
    [Theory]
    [InlineData("Column1,Column2\nValue1,Value2")]
    [InlineData("Column1;Column2\nValue1;Value2")]
    [InlineData("Column1|Column2\nValue1|Value2")]
    [InlineData("Column1\tColumn2\nValue1\tValue2")]
    public async Task DetectsSupportedDelimiter(string content)
    {
        var records = await ReadRecordsAsync(content, ';', true);

        Assert.Equal(2, records.Count);
        Assert.Equal(["Column1", "Column2"], records[0]);
        Assert.Equal(["Value1", "Value2"], records[1]);
    }

    [Fact]
    public async Task ReadsBook1CsvUsingEsMxCulture()
    {
        const string content = "\uFEFFTexto,Generico,Generico,Generico,Generico,number, decimal ,date\r\n" +
                               "Bla,Generico,\"1,02\",200,12/5/2026,25.00,\" 1,000,000.25 \",12/12/2020\r\n";
        var culture = CultureInfo.GetCultureInfo("es-MX");
        var previousCulture = CultureInfo.CurrentCulture;
        var previousUiCulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            var records = await ReadRecordsAsync(content, ';', true, culture);

            Assert.Equal(2, records.Count);
            Assert.Equal(8, records[0].Length);
            Assert.Equal(8, records[1].Length);
            Assert.Equal("1,02", records[1][2]);
            Assert.Equal(" 1,000,000.25 ", records[1][6]);

            var decimalField = new FormElementField
            {
                Component = FormComponent.Currency,
                DataType = FieldType.Decimal
            };
            var parsed = FormValuesService.HandleCurrencyComponent(decimalField, records[1][6]);

            Assert.Equal(1_000_000.25m, parsed);
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousUiCulture;
        }
    }

    [Fact]
    public async Task ReadsEscapedQuotesAndMultilineFields()
    {
        const string content = "Name,Notes\n\"Doe, Jane\",\"First line\nSecond \"\"quoted\"\" line\"";

        var records = await ReadRecordsAsync(content, ';', true);

        Assert.Equal(2, records.Count);
        Assert.Equal("Doe, Jane", records[1][0]);
        Assert.Equal("First line\nSecond \"quoted\" line", records[1][1]);
    }

    [Fact]
    public async Task FixedTabDelimiterDoesNotSplitNumericCommas()
    {
        const string content = "Name\tAmount\nValue\t1,000,000.25";

        var records = await ReadRecordsAsync(content, '\t', false);

        Assert.Equal(["Value", "1,000,000.25"], records[1]);
    }

    [Fact]
    public async Task PreservesBlankAndDelimiterOnlyRecordsForWorkerValidation()
    {
        const string content = "A;B\n\n;\n1;2";

        var records = await ReadRecordsAsync(content, ';', false);

        Assert.Equal(4, records.Count);
        Assert.Equal([""], records[1]);
        Assert.Equal(["", ""], records[2]);
    }

    private static async Task<List<string[]>> ReadRecordsAsync(string content, char separator,
        bool detectDelimiter, CultureInfo? culture = null)
    {
        using var textReader = new StringReader(content);
        var reader = new CsvImportationReader(textReader, culture ?? CultureInfo.InvariantCulture, separator,
            detectDelimiter);
        var records = new List<string[]>();

        await foreach (var record in reader.ReadRecordsAsync())
            records.Add(record);

        return records;
    }
}
