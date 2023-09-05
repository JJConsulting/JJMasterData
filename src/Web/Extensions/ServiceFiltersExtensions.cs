using JJMasterData.Web.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Web.Extensions;

public static class ServiceFiltersExtensions
{
    public static IServiceCollection AddActionFilters(this IServiceCollection services)
    {
        services.AddScoped<LookupParametersDecryptionFilter>();
        services.AddScoped<FormElementDecryptionFilter>();
        return services;
    }
}