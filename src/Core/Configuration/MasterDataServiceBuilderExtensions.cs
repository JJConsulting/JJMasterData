#nullable enable

using System;
using JJMasterData.Commons.Configuration;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Exportation.Abstractions;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.Core.Events.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.Configuration;

public static class MasterDataServiceBuilderExtensions
{
    public static MasterDataServiceBuilder WithFormEventHandlerFactory(this MasterDataServiceBuilder builder,Func<IServiceProvider, IDataDictionaryRepository> implementationFactory)
    {
        builder.Services.Replace(ServiceDescriptor.Transient(implementationFactory));

        return builder;
    }
    
    public static MasterDataServiceBuilder WithFormEventHandlerFactory<T>(this MasterDataServiceBuilder builder) where  T: class, IFormEventHandlerResolver
    {
        builder.Services.Replace(ServiceDescriptor.Transient<IFormEventHandlerResolver, T>());

        return builder;
    }
    
    public static MasterDataServiceBuilder WithPdfExportation<T>(this MasterDataServiceBuilder builder) where T : IPdfWriter
    {
        builder.Services.AddTransient(typeof(IPdfWriter), typeof(T));
        return builder;
    }
    
    public static MasterDataServiceBuilder WithDataDictionaryRepository<T>(this MasterDataServiceBuilder builder) where T : class, IDataDictionaryRepository 
    {
        builder.Services.Replace(ServiceDescriptor.Transient<IDataDictionaryRepository, T>());
        return builder;
    }
    
    public static MasterDataServiceBuilder WithDataDictionaryRepository(this MasterDataServiceBuilder builder, Func<IServiceProvider, IDataDictionaryRepository> implementationFactory) 
    {
        builder.Services.Replace(ServiceDescriptor.Transient(implementationFactory));
        return builder;
    }
    
    public static MasterDataServiceBuilder WithDatabaseDataDictionary(this MasterDataServiceBuilder builder)
    {
        builder.Services.Replace(ServiceDescriptor.Scoped<IDataDictionaryRepository, SqlDataDictionaryRepository>());
        return builder;
    }

    public static MasterDataServiceBuilder WithDatabaseDataDictionary(this MasterDataServiceBuilder builder, string connectionString, DataAccessProvider provider)
    {
        builder.WithEntityProvider(connectionString,provider);
        return WithDataDictionaryRepository(builder,serviceProvider => new SqlDataDictionaryRepository(serviceProvider.GetRequiredService<IEntityRepository>(),
            serviceProvider.GetRequiredService<IOptions<MasterDataCoreOptions>>()));
    }
    
    public static MasterDataServiceBuilder WithFileSystemDataDictionary(this MasterDataServiceBuilder builder)
    {
        builder.Services.AddOptions<FileSystemDataDictionaryOptions>().BindConfiguration("JJMasterData:DataDictionary");
        builder.Services.Replace(ServiceDescriptor.Transient<IDataDictionaryRepository, FileSystemDataDictionaryRepository>());
        return builder;
    }
    
    public static MasterDataServiceBuilder WithFileSystemDataDictionary(this MasterDataServiceBuilder builder, Action<FileSystemDataDictionaryOptions> options)
    {
        builder.Services.Configure(options);
        builder.Services.Replace(ServiceDescriptor.Transient<IDataDictionaryRepository, FileSystemDataDictionaryRepository>());
        return builder;
    }

    public static MasterDataServiceBuilder WithFileSystemDataDictionary(this MasterDataServiceBuilder builder, IConfiguration configuration)
    {
        builder.Services.AddOptions<FileSystemDataDictionaryOptions>().Bind(configuration);
        builder.Services.Replace(ServiceDescriptor.Transient<IDataDictionaryRepository, FileSystemDataDictionaryRepository>());
        return builder;
    }
    
    public static MasterDataServiceBuilder WithExcelExportation<T>(this MasterDataServiceBuilder builder) where T : class, IExcelWriter
    {
        builder.Services.Replace(ServiceDescriptor.Transient<IExcelWriter, T>());
        return builder;
    }

    public static MasterDataServiceBuilder WithTextExportation<T>(this MasterDataServiceBuilder builder) where T : class, ITextWriter
    {
        builder.Services.Replace(ServiceDescriptor.Transient<ITextWriter, T>());
        return builder;
    }
    public static MasterDataServiceBuilder WithExpressionProvider<T>(this MasterDataServiceBuilder builder) where T : class, IExpressionProvider
    {
        builder.Services.AddScoped<IExpressionProvider, T>();
        return builder;
    }
    
    public static MasterDataServiceBuilder WithActionPlugin<TPlugin>(this MasterDataServiceBuilder builder) where TPlugin : class, IPluginHandler
    {
        builder.Services.AddScoped<IPluginHandler, TPlugin>();
        
        return builder;
    }
}