using System;
using System.IO;
using JJMasterData.Commons.Configuration;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Core.Configuration;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Exports;
using JJMasterData.Core.DataManager.Exports.Abstractions;
using JJMasterData.Core.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace JJMasterData.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static JJMasterDataServiceBuilder AddJJMasterDataCore(this IServiceCollection services, string filePath = "appsettings.json")
    {
        var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile(filePath, optional: false, reloadOnChange: true)
        .Build();

        services.Configure<JJMasterDataCoreOptions>(configuration.GetJJMasterData());
        
        services.AddDefaultServices();
        
        return services.AddJJMasterDataCommons(configuration);
    }

    public static JJMasterDataServiceBuilder AddJJMasterDataCore(this IServiceCollection services,
        Action<JJMasterDataCoreOptions> configureCore,
        Action<JJMasterDataCommonsOptions> configureCommons, IConfiguration? loggingConfiguration = null)
    {
        services.Configure(configureCore);
        
        services.AddDefaultServices();
        return services.AddJJMasterDataCommons(configureCommons, loggingConfiguration);
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
        
        services.AddScoped<IDataDictionaryRepository, SqlDataDictionaryRepository>();
        services.AddTransient<IExcelWriter, ExcelWriter>();
        services.AddTransient<ITextWriter, DataManager.Exports.TextWriter>();
        
        services.AddFactories();
    }
}
