using System;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Cryptography.Abstractions;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Logging;
using JJMasterData.Commons.Logging.Db;
using JJMasterData.Commons.Logging.File;
using JJMasterData.Commons.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Configuration;

public class JJMasterDataServiceBuilder
{
    public IServiceCollection Services { get; }

    public JJMasterDataServiceBuilder(IServiceCollection services)
    {
        Services = services;
    }

    public JJMasterDataServiceBuilder AddDefaultServices(IConfiguration configuration)
    {
        Services.AddLocalization();
        Services.AddMemoryCache();
        Services.AddSingleton<ResourceManagerStringLocalizerFactory>();
        Services.AddSingleton<IStringLocalizerFactory, JJMasterDataStringLocalizerFactory>();
        Services.Add(new ServiceDescriptor(typeof(IStringLocalizer<>), typeof(JJMasterDataStringLocalizer<>), ServiceLifetime.Transient));
        Services.AddLogging(builder =>
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

        Services.AddTransient<IEntityRepository, EntityRepository>();
        Services.AddTransient<IEncryptionService, AesEncryptionService>();
        Services.AddTransient<JJMasterDataEncryptionService>();

        Services.AddSingleton<IBackgroundTask, BackgroundTask>();

        return this;
    }

    public JJMasterDataServiceBuilder WithBackgroundTask<T>() where T : class, IBackgroundTask
    {
        Services.Replace(ServiceDescriptor.Transient<IBackgroundTask, T>());
        return this;
    }

    public JJMasterDataServiceBuilder WithEntityRepository(string connectionString, DataAccessProvider provider)
    {
        return WithEntityRepository(serviceProvider =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var options = serviceProvider.GetRequiredService<IOptions<JJMasterDataCommonsOptions>>();
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            return new EntityRepository(configuration.GetConnectionString(connectionString), provider, options,
                loggerFactory);
        });
    }

    public JJMasterDataServiceBuilder WithEntityRepository(
        Func<IServiceProvider, IEntityRepository> implementationFactory)
    {
        Services.Replace(ServiceDescriptor.Transient(implementationFactory));
        return this;
    }
}