using JJMasterData.Core.Http.Abstractions;

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

    internal static ExportOptions LoadFromForm(IFormValues formValues, string componentName)
    {
        var expConfig = new ExportOptions();
        if (formValues[componentName + FileName] != null)
        {
            expConfig.FileExtension = (ExportFileExtension)int.Parse(formValues[componentName + FileName]);
            expConfig.IsLandScape = "true".Equals(formValues[componentName + TableOrientation]);
            expConfig.ExportFirstLine = "true".Equals(formValues[componentName + ExportTableFirstLine]);
            expConfig.ExportAllFields = "true".Equals(formValues[componentName + ExportAll]);
            expConfig.Delimiter = formValues[componentName + ExportDelimiter];
        }

        return expConfig;
    }


}
