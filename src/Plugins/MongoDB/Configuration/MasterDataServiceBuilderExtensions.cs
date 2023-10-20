using JJMasterData.Commons.Configuration;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.MongoDB.Models;
using JJMasterData.MongoDB.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Bson.Serialization;

namespace JJMasterData.MongoDB.Configuration;

public static class MasterDataServiceBuilderExtensions
{
    public static MasterDataServiceBuilder WithMongoDbDataDictionary(this MasterDataServiceBuilder builder)
    {
        ConfigureMappers();
        builder.Services.AddOptions<JJMasterDataMongoDBOptions>().BindConfiguration("JJMasterData:MongoDB");
        builder.Services.Replace(ServiceDescriptor.Transient<IDataDictionaryRepository, MongoDBDataDictionaryRepository>());
        return builder;
    }
    
    public static MasterDataServiceBuilder WithMongoDbDataDictionary(this MasterDataServiceBuilder builder, Action<JJMasterDataMongoDBOptions> options)
    {
        ConfigureMappers();
        builder.Services.Configure(options);
        builder.Services.Replace(ServiceDescriptor.Transient<IDataDictionaryRepository, MongoDBDataDictionaryRepository>());
        return builder;
    }

    public static MasterDataServiceBuilder WithMongoDbDataDictionary(this MasterDataServiceBuilder builder, IConfiguration configuration)
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


