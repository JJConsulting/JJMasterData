using System;
using JJMasterData.Commons.Configuration;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Core.Configuration;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Exports;
using JJMasterData.Core.DataManager.Exports.Abstractions;
using JJMasterData.Core.Options;
using JJMasterData.Core.UI.Components.FormView;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace JJMasterData.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static JJMasterDataServiceBuilder AddJJMasterDataCore(this IServiceCollection services)
    {
        services.AddOptions<JJMasterDataCommonsOptions>();
        
        services.AddDefaultServices();
        
        return services.AddJJMasterDataCommons();
    }

    public static JJMasterDataServiceBuilder AddJJMasterDataCore(this IServiceCollection services,
        Action<JJMasterDataCoreOptions> configureCore, IConfiguration loggingConfiguration = null)
    {
        var coreOptions = new JJMasterDataCoreOptions();

        configureCore(coreOptions);

        services.Configure(configureCore);
        
        services.AddDefaultServices();
        return services.AddJJMasterDataCommons(ConfigureJJMasterDataCommonsOptions, loggingConfiguration);

        void ConfigureJJMasterDataCommonsOptions(JJMasterDataCommonsOptions options)
        {
            options.ConnectionString = coreOptions.ConnectionString;
            options.ConnectionProvider = coreOptions.ConnectionProvider;
            options.LocalizationTableName = coreOptions.LocalizationTableName;
            options.ReadProcedurePattern = coreOptions.ReadProcedurePattern;
            options.WriteProcedurePattern = coreOptions.WriteProcedurePattern;
            options.SecretKey = coreOptions.SecretKey;
        }
    }

    public static JJMasterDataServiceBuilder AddJJMasterDataCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JJMasterDataCoreOptions>(configuration.GetJJMasterData());
        
        services.AddDefaultServices();
        
        return services.AddJJMasterDataCommons(configuration);
    }

    private static void AddDefaultServices(this IServiceCollection services)
    {
        
        services.AddHttpServices();
        services.AddDataDictionaryServices();
        services.AddDataManagerServices();
        services.AddEventHandlers();
        services.AddExpressionServices();
        services.AddActionServices();
        
        services.AddScoped<IDataDictionaryRepository, SqlDataDictionaryRepository>();
        
        services.AddScoped<IExcelWriter, ExcelWriter>();
        services.AddScoped<ITextWriter, TextWriter>();
        
        services.AddFactories();
    }
}
