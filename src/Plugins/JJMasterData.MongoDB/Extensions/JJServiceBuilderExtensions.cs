using JJMasterData.Commons.DI;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.MongoDB.Models;
using JJMasterData.MongoDB.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JJMasterData.MongoDB.Extensions;

public static class JJServiceBuilderExtensions
{
    public static JJServiceBuilder WithMongoDbDataDictionary(this JJServiceBuilder builder)
    {
        builder.Services.AddOptions<JJMasterDataMongoDBOptions>().BindConfiguration("JJMasterData:MongoDB");
        builder.Services.Replace(ServiceDescriptor.Transient<IDataDictionaryRepository, MongoDbDataDictionaryRepository>());
        return builder;
    }
    
    public static JJServiceBuilder WithMongoDbDataDictionary(this JJServiceBuilder builder, Action<JJMasterDataMongoDBOptions> options)
    {
        builder.Services.Configure(options);
        builder.Services.Replace(ServiceDescriptor.Transient<IDataDictionaryRepository, MongoDbDataDictionaryRepository>());
        return builder;
    }

    public static JJServiceBuilder WithMongoDbDataDictionary(this JJServiceBuilder builder, IConfiguration configuration)
    {
        builder.Services.AddOptions<JJMasterDataMongoDBOptions>().Bind(configuration);
        builder.Services.Replace(ServiceDescriptor.Transient<IDataDictionaryRepository, MongoDbDataDictionaryRepository>());
        return builder;
    }
    
}


