using JJMasterData.Core.UI.Components;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.Configuration;

public static class ActionsServiceExtensions
{
    public static IServiceCollection AddActionServices(this IServiceCollection services)
    {
        services.AddScoped<ActionScripts>();
        return services;
    }
}