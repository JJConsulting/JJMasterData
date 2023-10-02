using JJMasterData.Commons.Configuration;
using JJMasterData.Core.Configuration;

namespace JJMasterData.Pdf;
public static class Extensions
{
    public static JJMasterDataServiceBuilder WithPdfExportation(this JJMasterDataServiceBuilder builder)
    {
        return builder.WithPdfExportation<PdfWriter>();
    }
}