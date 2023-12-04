using System;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Data.Entity.Providers;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JJMasterData.Commons.Configuration;

public class MasterDataServiceBuilder
{
    public IServiceCollection Services { get; }

    public MasterDataServiceBuilder(IServiceCollection services)
    {
        Services = services;
    }
    
    public MasterDataServiceBuilder WithBackgroundTaskManager<T>() where T : class, IBackgroundTaskManager
    {
        Services.Replace(ServiceDescriptor.Transient<IBackgroundTaskManager, T>());
        return this;
    }

    public MasterDataServiceBuilder WithEntityProvider(string connectionString, DataAccessProvider provider)
    {
        Services.PostConfigure<MasterDataCommonsOptions>(options =>
        {
            options.ConnectionString = connectionString;
        });
        switch (provider)
        {
            case DataAccessProvider.SqlServer:
                Services.TryAddTransient<SqlServerReadProcedureScripts>();
                Services.TryAddTransient<SqlServerWriteProcedureScripts>();
                Services.TryAddTransient<SqlServerScripts>();
                WithEntityProvider<SqlServerProvider>();
                break;
            case DataAccessProvider.SqLite:
                WithEntityProvider<SQLiteProvider>();
                break;
            case DataAccessProvider.Oracle:
            case DataAccessProvider.OracleNetCore:
                WithEntityProvider<OracleProvider>();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(provider), provider, "Provider is not currently supported.");
        }

        return this;
    }

    public MasterDataServiceBuilder WithEntityProvider<T>() where T : EntityProviderBase
    {
        Services.Replace(ServiceDescriptor.Transient(typeof(EntityProviderBase),typeof(T)));
        return this;
    }
    
    public MasterDataServiceBuilder WithEntityRepository<T>() where T : IEntityRepository
    {
        Services.Replace(ServiceDescriptor.Transient(typeof(IEntityRepository),typeof(T)));
        return this;
    }
    
    public MasterDataServiceBuilder WithEntityRepository(
        Func<IServiceProvider, IEntityRepository> implementationFactory)
    {
        Services.Replace(ServiceDescriptor.Transient(implementationFactory));
        return this;
    }
}