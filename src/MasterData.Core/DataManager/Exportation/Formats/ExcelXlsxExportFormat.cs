using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Core.DataManager.Exportation.Abstractions;
using MiniExcelLibs;
using MiniExcelLibs.OpenXml;

namespace JJMasterData.Core.DataManager.Exportation.Formats;

internal sealed class ExcelXlsxExportFormat : IExportFormat<ExcelXlsxExportOptions>
{
    public ExportFormatConfiguration Configuration { get; } = new()
    {
        Id = "xlsx",
        DisplayName = "Excel (.xlsx)",
        FileExtension = "xlsx",
        ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        Options =
        [
            new ExportFormatOption
            {
                Name = nameof(ExcelXlsxExportOptions.ShowTableStyle),
                DisplayName = "Show table style",
                Kind = ExportFormatOptionKind.Boolean,
                DefaultValue = "true"
            }
        ]
    };

    public async Task WriteAsync(
        ExportContext context,
        ExcelXlsxExportOptions options,
        Stream output,
        CancellationToken cancellationToken)
    {
        await using var reader = new ExcelXlsxDataReader(context, cancellationToken);
        var configuration = new OpenXmlConfiguration
        {
            AutoFilter = context.IncludeHeader,
            FastMode = true,
            FreezeRowCount = context.IncludeHeader ? 1 : 0,
            TableStyles = context.IncludeHeader && options.ShowTableStyle
                ? TableStyles.Default
                : TableStyles.None
        };

        await output.SaveAsAsync(
            reader,
            printHeader: context.IncludeHeader,
            sheetName: "Sheet1",
            excelType: ExcelType.XLSX,
            configuration: configuration,
            cancellationToken: cancellationToken);
    }
}
