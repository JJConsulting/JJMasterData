using JJMasterData.Commons.Configuration;
using JJMasterData.Core.Web.Components.Scripts;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.Configuration;

public static class ScriptHelperServiceExtensions
{
    public static void AddScriptHelpers(this IServiceCollection services)
    {
        services.AddTransient<DataExportationScriptHelper>().AllowLazyInicialization();
        services.AddTransient<FormViewScriptHelper>().AllowLazyInicialization();
        services.AddTransient<DataPanelScriptHelper>().AllowLazyInicialization();
        services.AddTransient<GridViewScriptHelper>().AllowLazyInicialization();
        services.AddTransient<ScriptsHelper>();
    }
}