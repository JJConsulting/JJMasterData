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
public class JJServiceBuilder
{
    public IServiceCollection Services { get; }

    public JJServiceBuilder(IServiceCollection services)
    {
        Services = services;
    }

    public JJServiceBuilder AddDefaultServices(IConfiguration configuration)
    {
        Services.AddLocalization();
        Services.AddMemoryCache();
        Services.AddSingleton<ResourceManagerStringLocalizerFactory>();
        Services.AddSingleton<IStringLocalizerFactory,JJMasterDataStringLocalizerFactory>();
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
        
        Services.AddTransient<IEntityRepository,EntityRepository>();
        Services.AddTransient<IEncryptionService, AesEncryptionService>();
        Services.AddTransient<JJMasterDataEncryptionService>();
        
        Services.AddSingleton<IBackgroundTask, BackgroundTask>();
        
        return this;
    }

    public JJServiceBuilder WithBackgroundTask<T>() where T : class, IBackgroundTask
    {
        Services.Replace(ServiceDescriptor.Transient<IBackgroundTask, T>());
        return this;
    }
    
    public JJServiceBuilder WithEntityRepository(string connectionString, DataAccessProvider provider)
    {
        return WithEntityRepository(serviceProvider =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var options = serviceProvider.GetRequiredService<IOptions<JJMasterDataCommonsOptions>>();

            return new EntityRepository(configuration.GetConnectionString(connectionString),provider, options);
        });
    }
    
    public JJServiceBuilder WithEntityRepository(Func<IServiceProvider, IEntityRepository> implementationFactory)
    {
        Services.Replace(ServiceDescriptor.Transient(implementationFactory));
        return this;
    }
}