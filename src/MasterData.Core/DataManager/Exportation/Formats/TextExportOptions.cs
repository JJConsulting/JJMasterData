using System.ComponentModel.DataAnnotations;
using JJMasterData.Core.DataManager.Exportation.Abstractions;

namespace JJMasterData.Core.DataManager.Exportation.Formats;

public sealed class TextExportOptions : ExportFormatOptions
{
    [Display(Name = "Delimiter")]
    public TextExportDelimiter Delimiter { get; set; } = TextExportDelimiter.Tab;
}