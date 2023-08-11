using JJMasterData.Commons.Configuration;
using JJMasterData.Core.Extensions;

namespace JJMasterData.Pdf;
public static class Extensions
{
    public static JJMasterDataServiceBuilder WithPdfExportation(this JJMasterDataServiceBuilder builder)
    {
        return builder.WithPdfExportation<PdfWriter>();
    }
}