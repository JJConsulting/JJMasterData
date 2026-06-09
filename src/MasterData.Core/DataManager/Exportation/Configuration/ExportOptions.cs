using JJMasterData.Commons.Util;
using JJMasterData.Core.Extensions;
using Microsoft.AspNetCore.Http;

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

    internal static ExportOptions LoadFromForm(IHttpContextAccessor httpContextAccessor, string componentName)
    {
        var expConfig = new ExportOptions();

        if (!httpContextAccessor.HttpContext!.Request.HasFormContentType)
            return expConfig;
        
        var form = httpContextAccessor.HttpContext!.Request.Form;
        if (form.TryGetValue(componentName + FileName, out var fileName))
        {
            expConfig.FileExtension = (ExportFileExtension)int.Parse(fileName.ToString());
            expConfig.IsLandScape = StringManager.ParseBool(form[componentName + TableOrientation]);
            expConfig.ExportFirstLine = StringManager.ParseBool(form[componentName + ExportTableFirstLine]);
            expConfig.ExportAllFields = StringManager.ParseBool(form[componentName + ExportAll]);
            expConfig.Delimiter = form[componentName + ExportDelimiter];
        }

        return expConfig;
    }


}
