using JJMasterData.Core.Web.Components.Scripts;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.Configuration;

public static class ScriptHelperServiceExtensions
{
    public static void AddScriptHelpers(this IServiceCollection services)
    {
        services.AddTransient<DataExportationScriptHelper>();
        services.AddTransient<FormViewScriptHelper>();
        services.AddTransient<DataPanelScriptHelper>();
        services.AddTransient<GridViewScriptHelper>();
        services.AddTransient<ScriptsHelper>();
    }
}