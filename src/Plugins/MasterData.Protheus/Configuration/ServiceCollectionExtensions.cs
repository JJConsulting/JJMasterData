using JJMasterData.Protheus.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Protheus.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProtheusService(this IServiceCollection services)
    {
        return services.AddScoped<IProtheusService, ProtheusService>();
    } 
}