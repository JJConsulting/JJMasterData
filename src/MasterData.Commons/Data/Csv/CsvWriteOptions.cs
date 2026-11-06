#nullable enable
using System.Globalization;

namespace JJMasterData.Commons.Data.Csv;

/// <summary>Formatting for CSV output. Encoding is controlled by the destination writer.</summary>
public sealed class CsvWriteOptions
{
    public string Delimiter { get; init; } = "|";
    public bool IncludeHeader { get; init; }
    public string NewLine { get; init; } = "\r\n";
    public CultureInfo Culture { get; init; } = CultureInfo.InvariantCulture;
    public string DateTimeFormat { get; init; } = "yyyy-MM-dd HH:mm:ss";
}
