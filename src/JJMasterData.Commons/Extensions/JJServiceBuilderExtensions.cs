using JJMasterData.Commons.DI;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Commons.Extensions;

public static class JJServiceBuilderExtensions
{
    public static JJServiceBuilder AddJJMasterDataCommons(this IServiceCollection services)
    {
        var builder = new JJServiceBuilder(services);
        builder.AddDefaultServices();
        return builder;
    }
}