using Fluid;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Web.Components;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Web.Configuration;

public static class ActionsServiceExtensions
{
    public static IServiceCollection AddActionServices(this IServiceCollection services)
    {
        services.AddSingleton(_=>new FluidParser(new FluidParserOptions
        {
            AllowFunctions = true,
            AllowParentheses = true
        }));

        services.AddScoped<HtmlTemplateActionService>();
        services.AddScoped<ActionScripts>();
        return services;
    }
}