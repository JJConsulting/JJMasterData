using System;
using System.IO;
using JJMasterData.Commons.Configuration;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.DI;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.AuditLog;
using JJMasterData.Core.DataManager.Exports;
using JJMasterData.Core.DataManager.Exports.Abstractions;
using JJMasterData.Core.FormEvents;
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Core.Options;
using JJMasterData.Core.Web;
using JJMasterData.Core.Web.Components.Scripts;
using JJMasterData.Core.Web.Factories;
using JJMasterData.Core.Web.Http;
using JJMasterData.Core.Web.Http.Abstractions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

#if NET
using Microsoft.AspNetCore.Mvc.Infrastructure;
#endif

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
        Action<JJMasterDataCommonsOptions> configureCommons, IConfiguration loggingConfiguration = null)
    {
        services.Configure(configureCore);
        
        services.AddDefaultServices();
        return services.AddJJMasterDataCommons(configureCommons, loggingConfiguration);
    }

    public static JJServiceBuilder AddJJMasterDataCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JJMasterDataCoreOptions>(configuration.GetJJMasterData());
        
        services.AddDefaultServices();
        return services.AddJJMasterDataCommons(configuration);
    }

    private static void AddDefaultServices(this IServiceCollection services)
    {
        
        services.AddHttpServices();
        services.AddScriptHelpers();
        
        services.AddTransient<IFormEventResolver,FormEventResolver>();
        services.AddScoped<IDataDictionaryRepository, SqlDataDictionaryRepository>();
        
        services.AddTransient<IAuditLogService, AuditLogService>();
        services.AddTransient<IExcelWriter, ExcelWriter>();
        services.AddTransient<ITextWriter, DataManager.Exports.TextWriter>();
        
        services.AddTransient<JJMasterDataFactory>();
    }

    private static void AddScriptHelpers(this IServiceCollection services)
    {
        services.AddTransient<DataExpScriptHelper>();
        services.AddTransient<FormViewScriptHelper>();
        services.AddTransient<GridViewScriptHelper>();
        services.AddTransient<GridViewToolbarScriptHelper>();
    }
    
    private static void AddHttpServices(this IServiceCollection services)
    {
#if NET
        services.AddHttpContextAccessor();
        services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
#endif
        services.AddScoped<JJMasterDataUrlHelper>();

        services.AddScoped<IHttpSession, JJSession>();
        services.AddScoped<IHttpRequest, JJRequest>();
        services.AddScoped<IHttpResponse, JJResponse>();
        services.AddScoped<IHttpContext, JJHttpContext>();
    }
}
