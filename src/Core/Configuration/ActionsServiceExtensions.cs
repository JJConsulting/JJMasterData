using JJMasterData.Core.UI.Components.FormView;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.Configuration;

public static class ActionsServiceExtensions
{
    public static IServiceCollection AddActionServices(this IServiceCollection services)
    {
        services.AddTransient<ActionScripts>();
        return services;
    }
}