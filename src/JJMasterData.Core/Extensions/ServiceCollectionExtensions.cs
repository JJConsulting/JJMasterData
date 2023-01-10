using System;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Extensions;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataManager.Exports;
using JJMasterData.Core.DataManager.Exports.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using JJMasterData.Commons.Options;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.AuditLog;
using JJMasterData.Core.Facades;
using JJMasterData.Core.FormEvents;
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Core.Http;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.Options;
using JJMasterData.Core.WebComponents.Factories;

namespace JJMasterData.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static JJServiceBuilder AddJJMasterDataCore(this IServiceCollection services, string filePath = "appsettings.json")
    {
        var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile(filePath, optional: false, reloadOnChange: true)
        .Build();

        services.Configure<JJMasterDataCoreOptions>(configuration.GetJJMasterData());
        
        services.AddDefaultServices();
        
        return services.AddJJMasterDataCommons(configuration);
    }

    public static JJServiceBuilder AddJJMasterDataCore(this IServiceCollection services,
        Action<JJMasterDataCoreOptions> configureCore,
        Action<JJMasterDataCommonsOptions> configureCommons)
    {
        services.Configure(configureCore);
        
        services.AddDefaultServices();
        return services.AddJJMasterDataCommons(configureCommons);
    }

    public static JJServiceBuilder AddJJMasterDataCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JJMasterDataCoreOptions>(configuration.GetJJMasterData());
        
        services.AddDefaultServices();
        return services.AddJJMasterDataCommons(configuration);
    }

    private static void AddDefaultServices(this IServiceCollection services)
    {
#if NET
        services.AddHttpContextAccessor();
#endif
        services.AddScoped<IHttpSession, JJSession>();
        services.AddScoped<IHttpRequest, JJRequest>();
        services.AddScoped<IHttpResponse, JJResponse>();
        services.AddScoped<IHttpContext, JJHttpContext>();
        
        services.AddFactories();
        
        services.AddTransient<IFormEventResolver,FormEventResolver>();
        services.AddTransient<AuditLogService>();
        services.AddScoped<IDataDictionaryRepository, DatabaseDataDictionaryRepository>();
        
        services.AddTransient<IExportationWriter, ExcelWriter>();
        services.AddTransient<IExportationWriter, DataManager.Exports.TextWriter>();
        
        services.AddServicesFacades();
    }

    private static void AddFactories(this IServiceCollection services)
    {
        services.AddTransient<DataImpFactory>();
        services.AddTransient<DataPanelFactory>();
        services.AddTransient<FormViewFactory>();
        services.AddTransient<GridViewFactory>();
        services.AddTransient<WebComponentFactory>();
        services.AddTransient<SearchBoxFactory>();
        services.AddTransient<CollapsePanelFactory>();
        services.AddTransient<UploadAreaFactory>();
    }

    private static void AddServicesFacades(this IServiceCollection services)
    {
        services.AddTransient<RepositoryServicesFacade>();
    }
}
