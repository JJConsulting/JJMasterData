using JJMasterData.Commons.DI;
using JJMasterData.Core.Extensions;

namespace JJMasterData.Pdf;
public static class Extensions
{
    public static JJServiceBuilder WithPdfExportation(this JJServiceBuilder builder) => builder.WithPdfExportation<PdfWriter>();
}