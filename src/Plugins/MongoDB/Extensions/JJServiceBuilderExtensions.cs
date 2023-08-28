using JJMasterData.Commons.Configuration;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.MongoDB.Models;
using JJMasterData.MongoDB.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Bson.Serialization;

namespace JJMasterData.MongoDB.Extensions;

public static class JJServiceBuilderExtensions
{
    public static JJMasterDataServiceBuilder WithMongoDbDataDictionary(this JJMasterDataServiceBuilder builder)
    {
        ConfigureMappers();
        builder.Services.AddOptions<JJMasterDataMongoDBOptions>().BindConfiguration("JJMasterData:MongoDB");
        builder.Services.Replace(ServiceDescriptor.Transient<IDataDictionaryRepository, MongoDBDataDictionaryRepository>());
        return builder;
    }
    
    public static JJMasterDataServiceBuilder WithMongoDbDataDictionary(this JJMasterDataServiceBuilder builder, Action<JJMasterDataMongoDBOptions> options)
    {
        ConfigureMappers();
        builder.Services.Configure(options);
        builder.Services.Replace(ServiceDescriptor.Transient<IDataDictionaryRepository, MongoDBDataDictionaryRepository>());
        return builder;
    }

    public static JJMasterDataServiceBuilder WithMongoDbDataDictionary(this JJMasterDataServiceBuilder builder, IConfiguration configuration)
    {
        ConfigureMappers();
        builder.Services.AddOptions<JJMasterDataMongoDBOptions>().Bind(configuration);
        builder.Services.Replace(ServiceDescriptor.Transient<IDataDictionaryRepository, MongoDBDataDictionaryRepository>());
        return builder;
    }

    private static void ConfigureMappers()
    {
        BsonClassMap.RegisterClassMap<Element>(cm =>
        {
            cm.AutoMap();
            cm.MapProperty(p => p.Fields).SetElementName("ElementFields");
            cm.MapProperty(p => p.Relationships).SetElementName("ElementRelationships");
            cm.SetIsRootClass(true);
            cm.AddKnownType(typeof(FormElement));
        });
        
        BsonClassMap.RegisterClassMap<FormElement>(cm =>
        {
            cm.AutoMap();
            cm.MapProperty(p => p.Fields).SetElementName("FormElementFields");
            cm.MapProperty(p => p.Relationships).SetElementName("FormElementRelationships");
            cm.SetIsRootClass(false);
        });
    }
}


