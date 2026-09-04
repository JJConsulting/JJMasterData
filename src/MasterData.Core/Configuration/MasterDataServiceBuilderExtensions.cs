using System;
using JJMasterData.Commons.Configuration;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Exportation.Abstractions;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.Core.Events.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.Configuration;

public static class MasterDataServiceBuilderExtensions
{
    extension(MasterDataServiceBuilder builder)
    {
        public MasterDataServiceBuilder WithExportFormat<TFormat>() where TFormat : class, IExportFormat
        {
            builder.Services.AddScoped<IExportFormat, TFormat>();
            return builder;
        }

        public MasterDataServiceBuilder WithFormEventHandlerFactory(Func<IServiceProvider, IDataDictionaryRepository> implementationFactory)
        {
            builder.Services.Replace(ServiceDescriptor.Transient(implementationFactory));

            return builder;
        }

        public MasterDataServiceBuilder WithFormEventHandlerFactory<T>() where  T: class, IFormEventHandlerResolver
        {
            builder.Services.Replace(ServiceDescriptor.Transient<IFormEventHandlerResolver, T>());

            return builder;
        }

        public MasterDataServiceBuilder WithDataDictionaryRepository<T>() where T : class, IDataDictionaryRepository 
        {
            builder.Services.Replace(ServiceDescriptor.Transient<IDataDictionaryRepository, T>());
            return builder;
        }

        public MasterDataServiceBuilder WithDataDictionaryRepository(Func<IServiceProvider, IDataDictionaryRepository> implementationFactory) 
        {
            builder.Services.Replace(ServiceDescriptor.Transient(implementationFactory));
            return builder;
        }

        public MasterDataServiceBuilder WithDatabaseDataDictionary()
        {
            builder.Services.Replace(ServiceDescriptor.Scoped<IDataDictionaryRepository, SqlDataDictionaryRepository>());
            return builder;
        }

        public MasterDataServiceBuilder WithDatabaseDataDictionary(string connectionString, DataAccessProvider provider)
        {
            builder.WithEntityProvider(connectionString,provider);
            return builder.WithDataDictionaryRepository(serviceProvider => new SqlDataDictionaryRepository(serviceProvider.GetRequiredService<IEntityRepository>(),
                serviceProvider.GetRequiredService<IMemoryCache>(),serviceProvider.GetRequiredService<IOptionsSnapshot<MasterDataCoreOptions>>()));
        }

        public MasterDataServiceBuilder WithFileSystemDataDictionary()
        {
            builder.Services.AddOptions<FileSystemDataDictionaryOptions>().BindConfiguration("JJMasterData:DataDictionary");
            builder.Services.Replace(ServiceDescriptor.Transient<IDataDictionaryRepository, FileSystemDataDictionaryRepository>());
            return builder;
        }

        public MasterDataServiceBuilder WithFileSystemDataDictionary(Action<FileSystemDataDictionaryOptions> options)
        {
            builder.Services.Configure(options);
            builder.Services.Replace(ServiceDescriptor.Transient<IDataDictionaryRepository, FileSystemDataDictionaryRepository>());
            return builder;
        }

        public MasterDataServiceBuilder WithFileSystemDataDictionary(IConfiguration configuration)
        {
            builder.Services.AddOptions<FileSystemDataDictionaryOptions>().Bind(configuration);
            builder.Services.Replace(ServiceDescriptor.Transient<IDataDictionaryRepository, FileSystemDataDictionaryRepository>());
            return builder;
        }

        public MasterDataServiceBuilder WithExpressionProvider<T>() where T : class, IExpressionProvider
        {
            builder.Services.AddScoped<IExpressionProvider, T>();
            return builder;
        }

        public MasterDataServiceBuilder WithActionPlugin<TPlugin>() where TPlugin : class, IPluginHandler
        {
            builder.Services.AddScoped<IPluginHandler, TPlugin>();
        
            return builder;
        }
    }
}
