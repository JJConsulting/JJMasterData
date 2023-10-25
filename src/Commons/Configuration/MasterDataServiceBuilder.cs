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

public class MasterDataServiceBuilder
{
    public IServiceCollection Services { get; }

    public MasterDataServiceBuilder(IServiceCollection services)
    {
        Services = services;
    }
    
    public MasterDataServiceBuilder WithBackgroundTask<T>() where T : class, IBackgroundTaskManager
    {
        Services.Replace(ServiceDescriptor.Transient<IBackgroundTaskManager, T>());
        return this;
    }

    public MasterDataServiceBuilder WithEntityRepository(string connectionString, DataAccessProvider provider)
    {
        return WithEntityRepository(serviceProvider =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var options = serviceProvider.GetRequiredService<IOptions<MasterDataCommonsOptions>>();
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            return new EntityRepository(configuration.GetConnectionString(connectionString), provider, options,
                loggerFactory);
        });
    }

    public MasterDataServiceBuilder WithEntityRepository(
        Func<IServiceProvider, IEntityRepository> implementationFactory)
    {
        Services.Replace(ServiceDescriptor.Transient(implementationFactory));
        return this;
    }
}