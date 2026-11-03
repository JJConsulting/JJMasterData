using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.WebApi.Models;

namespace JJMasterData.WebApi.Test.Models;

public class MasterApiListResponseTests
{
    [Fact]
    public void ToCsv_ShouldUseResponseColumnOrder_WhenDatabaseColumnsHaveDifferentOrder()
    {
        var element = new FormElement { Name = "TestElement" };
        element.Fields.Add(new FormElementField { Name = "ID" });
        element.Fields.Add(new FormElementField { Name = "NAME" });
        var response = new MasterApiListResponse();
        response.SetData(element,
        [
            new Dictionary<string, object?>
            {
                ["NAME"] = "Rodrigo Macedo",
                ["ID"] = 7
            }
        ]);

        Assert.Equal("7|Rodrigo Macedo" + Environment.NewLine, response.ToCsv());
        Assert.NotNull(response.Fields);
        Assert.Equal(["id", "name"], response.Fields[0].Keys);
    }

    [Fact]
    public void ToCsv_ShouldQuoteSpecialValues_WhenUsingDefaultOptions()
    {
        var response = new MasterApiListResponse
        {
            TotalOfRecords = 12000,
            Fields =
            [
                new Dictionary<string, object?>
                {
                    ["id"] = 7,
                    ["description"] = " A|B\r\nC\rD\nE  ",
                    ["missing"] = null,
                    ["databaseNull"] = DBNull.Value,
                    ["date"] = new DateTime(2026, 11, 3, 14, 57, 0),
                    ["amount"] = 1234.56m
                }
            ]
        };

        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("pt-BR");

            var text = response.ToCsv();

            Assert.Equal(
                "7|\" A|B\r\nC\rD\nE  \"|||2026-11-03 14:57:00|1234.56" + Environment.NewLine,
                text);
            Assert.Equal(" A|B\r\nC\rD\nE  ", response.Fields[0]["description"]);
            Assert.Equal(12000, response.TotalOfRecords);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void ToCsv_ShouldApplyOptionsAndPreserveColumns_WhenConfigurationIsProvided()
    {
        var configuration = new CsvConfiguration(CultureInfo.GetCultureInfo("pt-BR"))
        {
            Delimiter = ";",
            HasHeaderRecord = true,
            NewLine = "\n",
            Quote = '\'',
            Escape = '\'',
            ShouldQuote = _ => true
        };
        var response = new MasterApiListResponse
        {
            Fields =
            [
                new Dictionary<string, object?> { ["name"] = "D'Ávila; Silva", ["amount"] = 12.5m },
                new Dictionary<string, object?> { ["amount"] = 2.5m, ["name"] = "Rodrigo" },
                new Dictionary<string, object?> { ["name"] = "Missing amount" }
            ]
        };

        var text = response.ToCsv(configuration);

        Assert.Equal(
            "'name';'amount'\n'D''Ávila; Silva';'12,5'\n'Rodrigo';'2,5'\n'Missing amount';''\n",
            text);
        using var reader = new StringReader(text);
        using var csv = new CsvReader(reader, configuration);
        Assert.True(csv.Read());
        csv.ReadHeader();
        Assert.True(csv.Read());
        Assert.Equal("D'Ávila; Silva", csv.GetField("name"));
        Assert.Equal(12.5m, csv.GetField<decimal>("amount"));
    }

    [Fact]
    public void ToCsv_ShouldReturnEmptyText_WhenFieldsAreMissing()
    {
        Assert.Equal(string.Empty, new MasterApiListResponse().ToCsv());
        Assert.Equal(string.Empty, new MasterApiListResponse { Fields = [] }.ToCsv());
    }
}
