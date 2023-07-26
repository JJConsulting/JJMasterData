using JJMasterData.Commons.Configuration;
using JJMasterData.Core.Web.Components.Scripts;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.Configuration;

public static class ScriptHelperServiceExtensions
{
    public static void AddScriptHelpers(this IServiceCollection services)
    {
        services.AddTransient<DataExportationScripts>().AllowLazyInicialization();
        services.AddTransient<FormViewScripts>().AllowLazyInicialization();
        services.AddTransient<DataPanelScripts>().AllowLazyInicialization();
        services.AddTransient<GridScripts>().AllowLazyInicialization();

    }
}