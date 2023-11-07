using JJMasterData.Commons.Configuration;
using JJMasterData.Core.Configuration;

namespace JJMasterData.NCalc.Configuration;

public static class MasterDataServiceBuilderExtensions
{
    public static MasterDataServiceBuilder WithNCalcExpressionProvider(this MasterDataServiceBuilder builder)
    {
        builder.WithExpressionProvider<NCalcExpressionProvider>();
        return builder;
    }
}