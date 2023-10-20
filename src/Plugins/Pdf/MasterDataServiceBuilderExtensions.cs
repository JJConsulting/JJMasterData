using JJMasterData.Commons.Configuration;
using JJMasterData.Core.Configuration;

namespace JJMasterData.Pdf;
public static class MasterDataServiceBuilderExtensions
{
    public static MasterDataServiceBuilder WithPdfExportation(this MasterDataServiceBuilder builder)
    {
        return builder.WithPdfExportation<PdfWriter>();
    }
}