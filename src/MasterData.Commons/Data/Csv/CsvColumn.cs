#nullable enable
namespace JJMasterData.Commons.Data.Csv;

/// <summary>A SQL result column and its optional CSV header.</summary>
public sealed record CsvColumn(string SourceName, string? Header = null);
