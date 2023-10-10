using JJMasterData.Commons.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WebOptimizer;

namespace JJMasterData.Web.Extensions;

public static class JJMasterDataServiceBuilderExtensions
{
    public static JJMasterDataServiceBuilder WithWebOptimizer(this JJMasterDataServiceBuilder serviceBuilder, Action<IAssetPipeline> configure)
    {
        var services = serviceBuilder.Services;

        services.RemoveAll<IAssetPipeline>();

        services.AddMasterDataWebOptimizer(configure);
        
        return serviceBuilder;
    }
}