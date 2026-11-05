using Fluid;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.UI.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JJMasterData.Core.Configuration;

public static class ActionsServiceExtensions
{
    public static IServiceCollection AddActionServices(this IServiceCollection services)
    {
        services.TryAddSingleton(_=>new FluidParser(new FluidParserOptions
        {
            AllowFunctions = true,
            AllowParentheses = true
        }));

        services.TryAddScoped<HtmlTemplateActionService>();
        services.TryAddScoped<ActionScripts>();
        return services;
    }
}