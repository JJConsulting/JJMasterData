using JJMasterData.Core.DataDictionary.Services;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.Configuration;

public static class DataDictionaryServiceExtensions
{
    public static IServiceCollection AddDataDictionaryServices(this IServiceCollection services)
    {
        services.AddTransient<ActionsService>();
        services.AddTransient<ApiService>();
        services.AddTransient<ElementService>();
        services.AddTransient<ElementImportService>();
        services.AddTransient<ElementExportService>();
        services.AddTransient<EntityService>();
        services.AddTransient<FieldService>();
        services.AddTransient<IndexesService>();
        services.AddTransient<UIOptionsService>();
        services.AddTransient<PanelService>();
        services.AddTransient<ClassGenerationService>();
        services.AddTransient<ScriptsService>();
        services.AddTransient<RelationshipsService>();
        services.AddTransient<DataDictionaryLocalizationService>();

        return services;
    }
}
