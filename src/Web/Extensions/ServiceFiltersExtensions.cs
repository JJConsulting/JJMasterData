using JJMasterData.Core.DataManager;
using JJMasterData.Web.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Web.Extensions;

public static class ServiceFiltersExtensions
{
    public static IServiceCollection AddActionFilters(this IServiceCollection services)
    {
        services.AddScoped<DictionaryNameDecryptionFilter>();
        services.AddScoped<ActionMapDecryptionFilter>();

        return services;
    }
}