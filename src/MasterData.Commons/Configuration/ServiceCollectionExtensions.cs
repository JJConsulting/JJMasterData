using System;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Data.Entity.Providers;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Security.Cryptography;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Commons.Tasks;
using JJMasterData.Commons.Util;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

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
        services.AddOptions<MasterDataCommonsOptions>()
            .BindConfiguration("JJMasterData")
            .Validate(o => !string.IsNullOrEmpty(o.ConnectionString),
                "Connection string is required at JJMasterData:ConnectionString at your configuration source.")
            .Validate(o => !string.IsNullOrEmpty(o.SecretKey),
                "Secret key is required at JJMasterData:SecretKey at your configuration source.")
            .ValidateOnStart();
        
        services.AddScoped<DataAccess>();

        services.AddOptions<SqlServerOptions>().BindConfiguration("JJMasterData");
        services.AddTransient<SqlServerReadProcedureScripts>();
        services.AddTransient<SqlServerWriteProcedureScripts>();
        services.AddTransient<SqlServerScripts>();

#pragma warning disable CS0618 // Type or member is obsolete
        services.AddTransient<EntityProviderBase>();
#pragma warning restore CS0618 // Type or member is obsolete
        services.AddTransient<IEntityProvider, SqlServerProvider>();
        services.AddTransient<IEntityRepository, EntityRepository>();
        services.AddTransient<IConnectionRepository, ConnectionRepository>();
        
        services.AddTransient<IEncryptionAlgorithm, AesEncryptionAlgorithm>();
        services.AddTransient<IEncryptionService, EncryptionService>();

        services.AddSingleton<IBackgroundTaskManager, BackgroundTaskManager>();
        
        services.AddScoped<RelativeDateFormatter>();
    }
}