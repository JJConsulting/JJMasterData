using JJMasterData.Commons.DI;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.MongoDB.Models;
using JJMasterData.MongoDB.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JJMasterData.MongoDB.Extensions;

public static class JJServiceBuilderExtensions
{
    public static JJServiceBuilder WithMongoDB(this JJServiceBuilder builder, Action<JJMasterDataMongoDBOptions> options)
    {
        
        builder.Services.Configure(options);
        builder.Services.Replace(ServiceDescriptor.Transient<IDictionaryRepository, MongoDictionaryRepository>());
        return builder;
    }
}


