using JJMasterData.Core.Web.Factories;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.Configuration;

public static class FactoriesServiceExtensions
{
    public static IServiceCollection AddFactories(this IServiceCollection services)
    {
        services.AddTransient<SearchBoxFactory>();
        services.AddTransient<DataImportationFactory>();
        services.AddTransient<DataExportationFactory>();
        services.AddTransient<JJMasterDataFactory>();
        return services;
    }
}