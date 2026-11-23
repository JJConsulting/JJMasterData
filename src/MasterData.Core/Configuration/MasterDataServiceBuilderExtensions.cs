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
using JJMasterData.Core.UI.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.Configuration;

public static class MasterDataServiceBuilderExtensions
{
    extension(MasterDataServiceBuilder builder)
    {
        /// <summary>Replaces the scoped store used for GridView preferences.</summary>
        public MasterDataServiceBuilder WithGridPreferenceStore<T>() where T : class, IGridPreferenceStore
        {
            builder.Services.Replace(ServiceDescriptor.Scoped<IGridPreferenceStore, T>());
            return builder;
        }

        /// <summary>Replaces the scoped store used for GridView preferences.</summary>
        public MasterDataServiceBuilder WithGridPreferenceStore(Func<IServiceProvider, IGridPreferenceStore> implementationFactory)
        {
            builder.Services.Replace(ServiceDescriptor.Scoped(implementationFactory));
            return builder;
        }

        /// <summary>
        /// Stores all grid preferences in the current ASP.NET Core session.
        /// The host must configure AddSession and UseSession.
        /// </summary>
        public MasterDataServiceBuilder WithGridSessionPreferenceStore()
        {
            return builder.WithGridPreferenceStore<SessionGridPreferenceStore>();
        }

        public MasterDataServiceBuilder WithExportFormat<TFormat>() where TFormat : class, IExportFormat
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Scoped<IExportFormat, TFormat>());
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
                serviceProvider.GetRequiredService<IOptionsSnapshot<MasterDataCoreOptions>>()));
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
