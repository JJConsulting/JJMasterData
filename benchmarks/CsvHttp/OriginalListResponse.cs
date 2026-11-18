// Snapshot from commit 042ad69d for the pre-optimization baseline.
using System.Globalization;
using System.Text.Json.Serialization;
using CsvHelper;
using CsvHelper.Configuration;
using JJMasterData.Core.DataDictionary.Models;


namespace CsvHttpBenchmarks;

internal class OriginalListResponse
{
    /// <summary>
    /// Quantidade total de registros no banco
    /// </summary>
    [JsonPropertyName("tot")]
    public int TotalOfRecords { get; set; }

    /// <summary>
    /// Tabela com os dados da pesquisa
    /// </summary>
    [JsonPropertyName("fields")]
    public Dictionary<string, object?>[]? Fields { get; set; }


    /// <summary>
    /// Formats the response fields as CSV. Defaults to pipe-delimited output without a header row.
    /// </summary>
    /// <param name="configuration">CSV writing options, including delimiter, headers, culture and quoting.</param>
    public string ToCsv(CsvConfiguration? configuration = null)
    {
        if (Fields == null || Fields.Length == 0)
            return string.Empty;

        configuration ??= new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = "|",
            HasHeaderRecord = false,
            NewLine = Environment.NewLine
        };

        using var writer = new StringWriter(configuration.CultureInfo);
        using var csv = new CsvWriter(writer, configuration);
        csv.Context.TypeConverterOptionsCache.GetOptions<DateTime>().Formats = ["yyyy-MM-dd HH:mm:ss"];

        var columns = Fields[0].Keys;
        if (configuration.HasHeaderRecord)
        {
            foreach (var column in columns)
                csv.WriteField(column);
            csv.NextRecord();
        }

        foreach (var row in Fields)
        {
            foreach (var column in columns)
            {
                row.TryGetValue(column, out var value);
                csv.WriteField(value == DBNull.Value ? null : value);
            }
            csv.NextRecord();
        }

        csv.Flush();
        return writer.ToString();
    }

    public void SetData(FormElement formElement, List<Dictionary<string, object?>> data)
    {
        var list = new List<Dictionary<string, object?>>();
        foreach (var row in data)
        {
            var cols = new Dictionary<string, object?>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var field in formElement.Fields)
            {
                var fieldName = formElement.ApiOptions.GetJsonFieldName(field.Name);
                var val = row[field.Name];
                if (val == DBNull.Value)
                    cols.Add(fieldName, null);
                else
                    cols.Add(fieldName, val);
            }

            list.Add(cols);
        }

        Fields = list.ToArray();
    }
}
