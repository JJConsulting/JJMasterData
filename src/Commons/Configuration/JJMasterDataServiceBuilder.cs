using System;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
    
    public JJMasterDataServiceBuilder WithBackgroundTask<T>() where T : class, IBackgroundTaskManager
    {
        Services.Replace(ServiceDescriptor.Transient<IBackgroundTaskManager, T>());
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