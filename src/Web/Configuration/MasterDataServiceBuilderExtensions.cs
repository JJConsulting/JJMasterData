using JJMasterData.Commons.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WebOptimizer;

namespace JJMasterData.Web.Configuration;

public static class MasterDataServiceBuilderExtensions
{
    public static MasterDataServiceBuilder WithWebOptimizer(this MasterDataServiceBuilder serviceBuilder, Action<IAssetPipeline> configure)
    {
        var services = serviceBuilder.Services;

        services.RemoveAll<IAssetPipeline>();

        services.AddJJMasterDataWebOptimizer(configure);
        
        return serviceBuilder;
    }
}