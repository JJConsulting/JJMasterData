using System;
using System.Data.Common;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Cryptography.Abstractions;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Commons.DI;
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
                builder.AddDbLoggerProvider();
                builder.AddFileLoggerProvider();
                builder.AddConfiguration(configuration.GetSection("Logging"));
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
        return WithEntityRepository(_ => new EntityRepository(connectionString, provider));
    }
    
    public JJServiceBuilder WithEntityRepository(Func<IServiceProvider, IEntityRepository> implementationFactory)
    {
        Services.Replace(ServiceDescriptor.Scoped(implementationFactory));
        return this;
    }
}