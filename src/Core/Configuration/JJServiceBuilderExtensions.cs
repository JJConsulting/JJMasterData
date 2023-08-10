#nullable enable

using System;
using JJMasterData.Commons.Configuration;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.DI;
using JJMasterData.Core.DataDictionary.FormEvents;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Exports.Abstractions;
using JJMasterData.Core.FormEvents;
using JJMasterData.Core.FormEvents.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.Extensions;

public static class JJServiceBuilderExtensions
{
    public static JJServiceBuilder WithFormEventHandlerFactory(this JJServiceBuilder builder,Func<IServiceProvider, IDataDictionaryRepository> implementationFactory)
    {
        builder.Services.Replace(ServiceDescriptor.Transient(implementationFactory));

        return builder;
    }
    
    public static JJServiceBuilder WithFormEventHandlerFactory<T>(this JJServiceBuilder builder) where  T: class, IFormEventHandlerFactory
    {
        builder.Services.Replace(ServiceDescriptor.Transient<IFormEventHandlerFactory, T>());

        return builder;
    }
    
    public static JJServiceBuilder WithPdfExportation<T>(this JJServiceBuilder builder) where T : IPdfWriter
    {
        builder.Services.AddTransient(typeof(IPdfWriter), typeof(T));
        return builder;
    }
    
    public static JJServiceBuilder WithDataDictionaryRepository<T>(this JJServiceBuilder builder) where T : class, IDataDictionaryRepository 
    {
        builder.Services.Replace(ServiceDescriptor.Transient<IDataDictionaryRepository, T>());
        return builder;
    }
    
    public static JJServiceBuilder WithDataDictionaryRepository(this JJServiceBuilder builder, Func<IServiceProvider, IDataDictionaryRepository> implementationFactory) 
    {
        builder.Services.Replace(ServiceDescriptor.Transient(implementationFactory));
        return builder;
    }
    
    public static JJServiceBuilder WithDatabaseDataDictionary(this JJServiceBuilder builder)
    {
        builder.Services.Replace(ServiceDescriptor.Scoped<IDataDictionaryRepository, SqlDataDictionaryRepository>());
        return builder;
    }

    public static JJServiceBuilder WithDatabaseDataDictionary(this JJServiceBuilder builder, string connectionString, DataAccessProvider provider)
    {
        return WithDataDictionaryRepository(builder,serviceProvider =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var options = serviceProvider.GetRequiredService<IOptions<JJMasterDataCommonsOptions>>();
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var entityRepository = new EntityRepository(configuration.GetConnectionString(connectionString),provider, options,loggerFactory);
            
            return new SqlDataDictionaryRepository(entityRepository,
                serviceProvider.GetRequiredService<IConfiguration>());
        });
    }
    
    public static JJServiceBuilder WithFileSystemDataDictionary(this JJServiceBuilder builder)
    {
        builder.Services.AddOptions<FileSystemDataDictionaryOptions>().BindConfiguration("JJMasterData:DataDictionary");
        builder.Services.Replace(ServiceDescriptor.Transient<IDataDictionaryRepository, FileSystemDataDictionaryRepository>());
        return builder;
    }
    
    public static JJServiceBuilder WithFileSystemDataDictionary(this JJServiceBuilder builder, Action<FileSystemDataDictionaryOptions> options)
    {
        builder.Services.Configure(options);
        builder.Services.Replace(ServiceDescriptor.Transient<IDataDictionaryRepository, FileSystemDataDictionaryRepository>());
        return builder;
    }

    public static JJServiceBuilder WithFileSystemDataDictionary(this JJServiceBuilder builder, IConfiguration configuration)
    {
        builder.Services.AddOptions<FileSystemDataDictionaryOptions>().Bind(configuration);
        builder.Services.Replace(ServiceDescriptor.Transient<IDataDictionaryRepository, FileSystemDataDictionaryRepository>());
        return builder;
    }
    
    public static JJServiceBuilder WithExcelExportation<T>(this JJServiceBuilder builder) where T : class, IExcelWriter
    {
        builder.Services.Replace(ServiceDescriptor.Transient<IExcelWriter, T>());
        return builder;
    }

    public static JJServiceBuilder WithTextExportation<T>(this JJServiceBuilder builder) where T : class, ITextWriter
    {
        builder.Services.Replace(ServiceDescriptor.Transient<ITextWriter, T>());
        return builder;
    }
}