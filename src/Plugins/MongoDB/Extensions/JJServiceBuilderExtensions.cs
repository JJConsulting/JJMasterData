using JJMasterData.Commons.Configuration;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.MongoDB.Models;
using JJMasterData.MongoDB.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JJMasterData.MongoDB.Extensions;

public static class JJServiceBuilderExtensions
{
    public static JJMasterDataServiceBuilder WithMongoDbDataDictionary(this JJMasterDataServiceBuilder builder)
    {
        builder.Services.AddOptions<JJMasterDataMongoDBOptions>().BindConfiguration("JJMasterData:MongoDB");
        builder.Services.Replace(ServiceDescriptor.Transient<IDataDictionaryRepository, MongoDbDataDictionaryRepository>());
        return builder;
    }
    
    public static JJMasterDataServiceBuilder WithMongoDbDataDictionary(this JJMasterDataServiceBuilder builder, Action<JJMasterDataMongoDBOptions> options)
    {
        builder.Services.Configure(options);
        builder.Services.Replace(ServiceDescriptor.Transient<IDataDictionaryRepository, MongoDbDataDictionaryRepository>());
        return builder;
    }

    public static JJMasterDataServiceBuilder WithMongoDbDataDictionary(this JJMasterDataServiceBuilder builder, IConfiguration configuration)
    {
        builder.Services.AddOptions<JJMasterDataMongoDBOptions>().Bind(configuration);
        builder.Services.Replace(ServiceDescriptor.Transient<IDataDictionaryRepository, MongoDbDataDictionaryRepository>());
        return builder;
    }
    
}


