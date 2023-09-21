using System;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Cryptography.Abstractions;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Logging;
using JJMasterData.Commons.Logging.Db;
using JJMasterData.Commons.Logging.File;
using JJMasterData.Commons.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Commons.Configuration;

public static class ServiceCollectionExtensions
{
    public static JJMasterDataServiceBuilder AddJJMasterDataCommons(this IServiceCollection services)
    {
        var builder = new JJMasterDataServiceBuilder(services);
        services.AddOptions<JJMasterDataCommonsOptions>();

        services.AddJJMasterDataCommonsServices();
        
        return builder;
    }

    public static JJMasterDataServiceBuilder AddJJMasterDataCommons(this IServiceCollection services, IConfiguration configuration)
    {
        var builder = new JJMasterDataServiceBuilder(services);

        builder.Services.Configure<JJMasterDataCommonsOptions>(configuration.GetJJMasterData());
        
        services.AddJJMasterDataCommonsServices(configuration);

        return builder;
    }

    public static JJMasterDataServiceBuilder AddJJMasterDataCommons(this IServiceCollection services, Action<JJMasterDataCommonsOptions> configure, IConfiguration loggingConfiguration = null)
    {
        var builder = new JJMasterDataServiceBuilder(services);

        services.AddJJMasterDataCommonsServices(loggingConfiguration);
        services.Configure(configure);

        return builder;
    }
    
    private static IServiceCollection AddJJMasterDataCommonsServices(this IServiceCollection services,IConfiguration configuration = null)
    {
        services.AddLocalization();
        services.AddMemoryCache();
        services.AddSingleton<ResourceManagerStringLocalizerFactory>();
        services.AddSingleton<IStringLocalizerFactory, JJMasterDataStringLocalizerFactory>();
        services.Add(new ServiceDescriptor(typeof(IStringLocalizer<>), typeof(JJMasterDataStringLocalizer<>), ServiceLifetime.Transient));
        services.AddLogging(builder =>
        {
            if (configuration != null)
            {
                var loggingOptions = configuration.GetSection("Logging");
                builder.AddConfiguration(loggingOptions);

                if (loggingOptions.GetSection(DbLoggerProvider.ProviderName) != null)
                    builder.AddDbLoggerProvider();

                if (loggingOptions.GetSection(FileLoggerProvider.ProviderName) != null)
                    builder.AddFileLoggerProvider();
            }
        });

        services.AddTransient<IEntityRepository, EntityRepository>();
        services.AddTransient<IEncryptionAlgorithm, AesEncryptionAlgorithm>();
        services.AddTransient<IEncryptionService,EncryptionService>();

        services.AddSingleton<IBackgroundTaskManager, BackgroundTaskManager>();

        return services;
    }
}