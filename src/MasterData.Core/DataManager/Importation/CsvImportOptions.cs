using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace JJMasterData.Core.DataManager.Importation;

public sealed class CsvImportOptions : ImportFormatOptions
{
    protected internal override string Id => "csv";
    protected internal override string DisplayName => "CSV";
    protected internal override IReadOnlyList<string> FileExtensions => [".csv", ".txt", ".tsv"];
    protected internal override IReadOnlyList<string> ContentTypes =>
        ["text/csv", "text/plain", "text/tab-separated-values", "application/csv", "application/vnd.ms-excel"];

    [Display(Name = "Delimiter")]
    public CsvImportDelimiter Delimiter { get; set; } = CsvImportDelimiter.Semicolon;

    [Display(Name = "Detect delimiter")]
    public bool DetectDelimiter { get; set; } = true;
}