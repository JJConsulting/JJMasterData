#nullable disable warnings

namespace JJMasterData.Core.DataManager.Exportation.Configuration;

/// <summary>
/// Classe responsável por configurar a exportação dos dados da JJGridView
/// </summary>
public class ExportOptions
{
    internal const string FileName = "_export_table_file";
    internal const string TableOrientation = "_export_table_orientation";
    internal const string ExportTableFirstLine = "_export_table_firstline";
    internal const string ExportAll = "_export_table_all";
    internal const string ExportDelimiter = "_export_table_delimiter";

    public ExportFileExtension FileExtension { get; set; } = ExportFileExtension.CSV;
    public bool ExportFirstLine { get; set; } = true;
    public bool ExportAllFields { get; set; } = true;
    public bool IsLandScape { get; set; } = false;
    public string Delimiter { get; set; } = ";";

}
