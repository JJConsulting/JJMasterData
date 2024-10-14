using System;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Data.Entity.Providers;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Logging;
using JJMasterData.Commons.Logging.Db;
using JJMasterData.Commons.Security.Cryptography;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Commons.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace JJMasterData.Commons.Configuration;

public static class ServiceCollectionExtensions
{
    public static MasterDataServiceBuilder AddJJMasterDataCommons(this IServiceCollection services)
    {
        var builder = new MasterDataServiceBuilder(services);

        services.AddMasterDataCommonsServices();

        return builder;
    }

    public static MasterDataServiceBuilder AddJJMasterDataCommons(this IServiceCollection services,
        IConfiguration configuration)
    {
        var builder = new MasterDataServiceBuilder(services);

        builder.Services.Configure<MasterDataCommonsOptions>(configuration.GetJJMasterData());

        services.AddMasterDataCommonsServices();

        return builder;
    }

    public static MasterDataServiceBuilder AddJJMasterDataCommons(this IServiceCollection services,
        Action<MasterDataCommonsOptions> configure)
    {
        var builder = new MasterDataServiceBuilder(services);

        services.AddMasterDataCommonsServices();
        if (configure != null) 
            services.PostConfigure(configure);

        return builder;
    }

    private static void AddMasterDataCommonsServices(this IServiceCollection services)
    {
        services.AddOptions<MasterDataCommonsOptions>().BindConfiguration("JJMasterData");
        services.AddOptions<DbLoggerOptions>().BindConfiguration("Logging:Database");
        services.AddOptions<LoggerFilterOptions>().BindConfiguration("Logging");

        services.AddLocalization();
        services.AddMemoryCache();
        services.AddSingleton<ResourceManagerStringLocalizerFactory>();
        services.AddSingleton<IStringLocalizerFactory, MasterDataStringLocalizerFactory>();
        services.AddTransient(typeof(IStringLocalizer<>), typeof(MasterDataStringLocalizer<>));
        services.AddLogging(builder =>
        {
            //We can't have control when Db and File are enabled/disabled dynamically
            builder.AddDbLoggerProvider();
            builder.AddFileLoggerProvider();
            builder.AddConfiguration();
        });

        services.AddScoped<DataAccess>();
        services.AddScoped<SqlServerInfo>();

        services.AddTransient<SqlServerReadProcedureScripts>();
        services.AddTransient<SqlServerWriteProcedureScripts>();
        services.AddTransient<SqlServerScripts>();

        services.AddTransient<EntityProviderBase, SqlServerProvider>();
        services.AddTransient<IEntityRepository, EntityRepository>();

        services.AddTransient<IEncryptionAlgorithm, AesEncryptionAlgorithm>();
        services.AddTransient<IEncryptionService, EncryptionService>();

        services.AddSingleton<IBackgroundTaskManager, BackgroundTaskManager>();
    }
}