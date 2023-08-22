#nullable enable

using System;
using JJMasterData.Commons.Configuration;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Exports.Abstractions;
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Core.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.Extensions;

public static class JJMasterDataServiceBuilderExtensions
{
    public static JJMasterDataServiceBuilder WithFormEventHandlerFactory(this JJMasterDataServiceBuilder builder,Func<IServiceProvider, IDataDictionaryRepository> implementationFactory)
    {
        builder.Services.Replace(ServiceDescriptor.Transient(implementationFactory));

        return builder;
    }
    
    public static JJMasterDataServiceBuilder WithFormEventHandlerFactory<T>(this JJMasterDataServiceBuilder builder) where  T: class, IFormEventHandlerFactory
    {
        builder.Services.Replace(ServiceDescriptor.Transient<IFormEventHandlerFactory, T>());

        return builder;
    }
    
    public static JJMasterDataServiceBuilder WithPdfExportation<T>(this JJMasterDataServiceBuilder builder) where T : IPdfWriter
    {
        builder.Services.AddTransient(typeof(IPdfWriter), typeof(T));
        return builder;
    }
    
    public static JJMasterDataServiceBuilder WithDataDictionaryRepository<T>(this JJMasterDataServiceBuilder builder) where T : class, IDataDictionaryRepository 
    {
        builder.Services.Replace(ServiceDescriptor.Transient<IDataDictionaryRepository, T>());
        return builder;
    }
    
    public static JJMasterDataServiceBuilder WithDataDictionaryRepository(this JJMasterDataServiceBuilder builder, Func<IServiceProvider, IDataDictionaryRepository> implementationFactory) 
    {
        builder.Services.Replace(ServiceDescriptor.Transient(implementationFactory));
        return builder;
    }
    
    public static JJMasterDataServiceBuilder WithDatabaseDataDictionary(this JJMasterDataServiceBuilder builder)
    {
        builder.Services.Replace(ServiceDescriptor.Scoped<IDataDictionaryRepository, SqlDataDictionaryRepository>());
        return builder;
    }

    public static JJMasterDataServiceBuilder WithDatabaseDataDictionary(this JJMasterDataServiceBuilder builder, string connectionString, DataAccessProvider provider)
    {
        return WithDataDictionaryRepository(builder,serviceProvider =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var options = serviceProvider.GetRequiredService<IOptions<JJMasterDataCommonsOptions>>();
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var entityRepository = new EntityRepository(configuration.GetConnectionString(connectionString),provider, options,loggerFactory);
            
            return new SqlDataDictionaryRepository(entityRepository,
                serviceProvider.GetRequiredService<IOptions<JJMasterDataCoreOptions>>());
        });
    }
    
    public static JJMasterDataServiceBuilder WithFileSystemDataDictionary(this JJMasterDataServiceBuilder builder)
    {
        builder.Services.AddOptions<FileSystemDataDictionaryOptions>().BindConfiguration("JJMasterData:DataDictionary");
        builder.Services.Replace(ServiceDescriptor.Transient<IDataDictionaryRepository, FileSystemDataDictionaryRepository>());
        return builder;
    }
    
    public static JJMasterDataServiceBuilder WithFileSystemDataDictionary(this JJMasterDataServiceBuilder builder, Action<FileSystemDataDictionaryOptions> options)
    {
        builder.Services.Configure(options);
        builder.Services.Replace(ServiceDescriptor.Transient<IDataDictionaryRepository, FileSystemDataDictionaryRepository>());
        return builder;
    }

    public static JJMasterDataServiceBuilder WithFileSystemDataDictionary(this JJMasterDataServiceBuilder builder, IConfiguration configuration)
    {
        builder.Services.AddOptions<FileSystemDataDictionaryOptions>().Bind(configuration);
        builder.Services.Replace(ServiceDescriptor.Transient<IDataDictionaryRepository, FileSystemDataDictionaryRepository>());
        return builder;
    }
    
    public static JJMasterDataServiceBuilder WithExcelExportation<T>(this JJMasterDataServiceBuilder builder) where T : class, IExcelWriter
    {
        builder.Services.Replace(ServiceDescriptor.Transient<IExcelWriter, T>());
        return builder;
    }

    public static JJMasterDataServiceBuilder WithTextExportation<T>(this JJMasterDataServiceBuilder builder) where T : class, ITextWriter
    {
        builder.Services.Replace(ServiceDescriptor.Transient<ITextWriter, T>());
        return builder;
    }
}