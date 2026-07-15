using System;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Data.Entity.Providers;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Security.Cryptography;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Commons.Storage;
using JJMasterData.Commons.Tasks;
using JJMasterData.Commons.Util;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JJMasterData.Commons.Configuration;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public MasterDataServiceBuilder AddJJMasterDataCommons()
        {
            var builder = new MasterDataServiceBuilder(services);

            services.AddMasterDataCommonsServices();

            return builder;
        }

        public MasterDataServiceBuilder AddJJMasterDataCommons(IConfiguration configuration)
        {
            var builder = new MasterDataServiceBuilder(services);

            builder.Services.Configure<MasterDataCommonsOptions>(configuration.GetJJMasterData());

            services.AddMasterDataCommonsServices();

            return builder;
        }

        public MasterDataServiceBuilder AddJJMasterDataCommons(Action<MasterDataCommonsOptions> configure)
        {
            var builder = new MasterDataServiceBuilder(services);

            services.AddMasterDataCommonsServices();
            if (configure != null) 
                services.PostConfigure(configure);

            return builder;
        }

        private void AddMasterDataCommonsServices()
        {
            services.AddOptions<MasterDataCommonsOptions>()
                .BindConfiguration("JJMasterData")
                .Validate(o => !string.IsNullOrEmpty(o.ConnectionString),
                    "Connection string is required at JJMasterData:ConnectionString at your configuration source.")
                .ValidateOnStart();
        
            services.TryAddScoped<DataAccess>();

            services.AddDataProtection(); 
            
            services.AddOptions<SqlServerOptions>().BindConfiguration("JJMasterData");
            
            services.TryAddTransient<SqlServerReadProcedureScripts>();
            services.TryAddTransient<SqlServerWriteProcedureScripts>();
            services.TryAddTransient<SqlServerScripts>();
            
            services.TryAddTransient<IEntityProvider, SqlServerProvider>();
            services.TryAddTransient<IEntityRepository, EntityRepository>();
            services.TryAddTransient<IConnectionRepository, ConnectionRepository>();
            
            services.TryAddTransient<IEncryptionService, EncryptionService>();
            
            services.TryAddTransient<RelativeDateFormatter>();
            
            services.TryAddSingleton<IFileStorage, DiskFileStorage>();
            services.TryAddSingleton<IBackgroundTaskManager, BackgroundTaskManager>();

        }
    }
}
