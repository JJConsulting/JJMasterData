using JJMasterData.Commons.Configuration;
using JJMasterData.ConsoleApp.Models.FormElementMigration;
using JJMasterData.ConsoleApp.Repository;
using JJMasterData.ConsoleApp.Services;
using JJMasterData.ConsoleApp.Writers;
using JJMasterData.Core.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.ConsoleApp.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJJMasterDataConsoleServices(this IServiceCollection services)
    {
        var builder = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory).AddJsonFile("appsettings.json", false, false);

        IConfiguration configuration = builder.Build();

        services.AddSingleton(configuration);

        var tableName = configuration.GetJJMasterData().GetValue<string>("DataDictionaryTableName")!;

        services.AddJJMasterDataCore(coreOptions =>
        {
            coreOptions.DataDictionaryTableName = tableName;
        }, commonsOptions =>
        {
            commonsOptions.WriteProcedurePattern = configuration.GetJJMasterData().GetValue<string>("PrefixGetProc")!;
            commonsOptions.ReadProcedurePattern = configuration.GetJJMasterData().GetValue<string>("PrefixSetProc")!;
        });

        services.AddTransient<MetadataRepository>();
        services.AddTransient<FormElementMigrationService>();

        services.AddTransient<ImportService>();
        
        services.AddTransient<JsonSchemaService>();
        services.AddTransient<JJMasterDataOptionsWriter>();
        services.AddTransient<FormElementWriter>();
        
        return services;
    }
}