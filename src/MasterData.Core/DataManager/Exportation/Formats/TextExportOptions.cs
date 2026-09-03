using System.ComponentModel.DataAnnotations;

namespace JJMasterData.Core.DataManager.Exportation.Formats;

public sealed class TextExportOptions : ExportFormatOptions
{
    protected internal override string Id => "txt";
    protected internal override string DisplayName => "Text";
    protected internal override string FileExtension => "txt";
    protected internal override string ContentType => "text/plain";

    [Display(Name = "Delimiter")]
    public TextExportDelimiter Delimiter { get; set; } = TextExportDelimiter.Tab;
}