using JJMasterData.Core.DataDictionary.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JJMasterData.Core.Configuration;

public static class DataDictionaryServiceExtensions
{
    public static IServiceCollection AddDataDictionaryServices(this IServiceCollection services)
    {
        services.TryAddScoped<DataDictionaryService>();
        services.TryAddTransient<ActionsService>();
        services.TryAddTransient<ApiService>();
        services.TryAddTransient<ElementService>();
        services.TryAddTransient<ElementImportService>();
        services.TryAddTransient<ElementExportService>();
        services.TryAddTransient<EntityService>();
        services.TryAddTransient<FieldService>();
        services.TryAddTransient<IndexesService>();
        services.TryAddTransient<UIOptionsService>();
        services.TryAddTransient<PanelService>();
        services.TryAddTransient<ClassGenerationService>();
        services.TryAddTransient<ScriptsService>();
        services.TryAddTransient<RelationshipsService>();
        services.TryAddTransient<FormElementRulesService>();
        services.TryAddTransient<DataDictionaryLocalizationService>();

        return services;
    }
}
