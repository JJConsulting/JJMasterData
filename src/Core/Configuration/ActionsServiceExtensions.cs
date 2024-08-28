using Fluid;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.UI.Components;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.Configuration;

public static class ActionsServiceExtensions
{
    public static IServiceCollection AddActionServices(this IServiceCollection services)
    {
        services.AddSingleton(_=>new FluidParser(new FluidParserOptions
        {
            AllowFunctions = true
        }));

        services.AddScoped<HtmlTemplateService>();
        services.AddScoped<ActionScripts>();
        return services;
    }
}