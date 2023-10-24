using JJMasterData.Commons.Configuration;
using JJMasterData.ConsoleApp.Repository;
using JJMasterData.ConsoleApp.Services;
using JJMasterData.ConsoleApp.Writers;
using JJMasterData.Core.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.ConsoleApp.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJJMasterDataConsoleServices(this IServiceCollection services)
    {
        services.AddJJMasterDataCore();

        services.AddTransient<MetadataRepository>();
        services.AddTransient<FormElementMigrationService>();
        services.AddTransient<ExpressionsMigrationService>();
        
        services.AddTransient<ImportService>();
        
        services.AddTransient<JsonSchemaService>();
        services.AddTransient<JJMasterDataOptionsWriter>();
        services.AddTransient<FormElementWriter>();
        
        return services;
    }
}