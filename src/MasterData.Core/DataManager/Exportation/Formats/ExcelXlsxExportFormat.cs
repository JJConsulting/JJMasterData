using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Core.DataManager.Exportation;
using JJMasterData.Core.DataManager.Exportation.Abstractions;
using MiniExcelLibs;
using MiniExcelLibs.OpenXml;

namespace JJMasterData.Core.DataManager.Exportation.Formats;

internal sealed class ExcelXlsxExportFormat : IExportFormat<ExcelXlsxExportOptions>
{
    public string Id => "xlsx";
    public string DisplayName => "Excel (.xlsx)";
    public string FileExtension => "xlsx";
    
    public async Task WriteAsync(
        ExportContext context,
        ExcelXlsxExportOptions options,
        Stream output,
        CancellationToken cancellationToken)
    {
        await using var reader = new ExcelXlsxDataReader(context, cancellationToken);
        var configuration = new OpenXmlConfiguration
        {
            AutoFilter = options.IncludeFirstRowAsHeader,
            FastMode = true,
            FreezeRowCount = options.IncludeFirstRowAsHeader ? 1 : 0,
            TableStyles = options.IncludeFirstRowAsHeader && options.ShowTableStyle
                ? TableStyles.Default
                : TableStyles.None
        };

        await output.SaveAsAsync(
            reader,
            printHeader: options.IncludeFirstRowAsHeader,
            sheetName: "Sheet1",
            excelType: ExcelType.XLSX,
            configuration: configuration,
            cancellationToken: cancellationToken);
    }
}
