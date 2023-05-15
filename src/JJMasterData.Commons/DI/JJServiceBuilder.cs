using System;
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
        
        Services.AddLogging(builder =>
        {
            if (configuration != null)
            {
                builder.AddDbLoggerProvider();
                builder.AddFileLoggerProvider();
                builder.AddConfiguration(configuration.GetSection("Logging"));
            }
        });
        
        Services.AddScoped<DataAccess>();
        Services.AddScoped<IEntityRepository,EntityRepository>();
        
        Services.AddTransient<IEncryptionService, AesEncryptionService>();
        Services.AddTransient<JJMasterDataEncryptionService>();
        
        Services.AddTransient<ILocalizationProvider, JJMasterDataLocalizationProvider>();
        Services.AddSingleton<IBackgroundTask, BackgroundTask>();
        
        return this;
    }

    public JJServiceBuilder WithBackgroundTask<T>() where T : class, IBackgroundTask
    {
        Services.Replace(ServiceDescriptor.Transient<IBackgroundTask, T>());
        return this;
    }
    
    public JJServiceBuilder WithDataAccess(Func<IServiceProvider, DataAccess> implementationFactory)
    {
        Services.Replace(ServiceDescriptor.Scoped(implementationFactory));
        return this;
    }
    
    public JJServiceBuilder WithLocalizationProvider<T>() where T : class, ILocalizationProvider
    {
        Services.Replace(ServiceDescriptor.Transient<ILocalizationProvider, T>());
        return this;
    }
}