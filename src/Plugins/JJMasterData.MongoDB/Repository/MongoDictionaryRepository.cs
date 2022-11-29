using System.Collections;
using System.Data;
using JJMasterData.Commons.Extensions;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.MongoDB.Extensions;
using JJMasterData.MongoDB.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace JJMasterData.MongoDB.Repository;

public class MongoDictionaryRepository : IDictionaryRepository
{
    private readonly IMongoCollection<MongoDBMetadata> _metadataCollection;

    public MongoDictionaryRepository(IOptions<JJMasterDataMongoDBOptions> options)
    {
        var mongoClient = new MongoClient(
            options.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            options.Value.DatabaseName);

        _metadataCollection = mongoDatabase.GetCollection<MongoDBMetadata>(
            options.Value.CollectionName);
    }
    public void ExecInitialSetup()
    {
        // MongoDB don't need initial setup. It automatically handle database and table creation at runtime.
    }

    public Metadata GetMetadata(string elementName)
    {
        return _metadataCollection.Find(metadata=> metadata.Table.Name == elementName).FirstOrDefault();
    }

    public IList<Metadata> GetMetadataList(bool? sync)
    {
        var dbMetadataCollection =  _metadataCollection.Find(_ => true).ToList();

        return dbMetadataCollection.Select(MongoDBMetadataMapper.FromMongoDBMetadata).ToList();
    }

    public IEnumerable<string> GetNameList()
    {
        return _metadataCollection.Find(_ => true).ToList().Select(metadata => metadata.Table.Name).ToList();
    }

    public DataTable GetDataTable(IDictionary filters, string orderby, int regperpage, int pag, ref int tot)
    {
        var bson = new BsonDocument(filters);
        var metadataList = _metadataCollection
            .Find(_=>true)
            // .Sort(orderby)
            // .Limit(regperpage)
            // .Skip(pag)
            .ToList();

        tot = metadataList.Count;
        
        return metadataList.ToDataTable();
    }

    public bool Exists(string elementName)
    {
        return _metadataCollection.Find(metadata => metadata.Table.Name == elementName).ToList().Count > 0;
    }

    public void InsertOrReplace(Metadata metadata)
    {
        var mongoDbMetadata = MongoDBMetadataMapper.FromMetadata(metadata);
        
        _metadataCollection.ReplaceOne(
            filter: m=>metadata.Table.Name == m.Table.Name,
            options: new ReplaceOptions { IsUpsert = true },
            replacement: mongoDbMetadata);
    }

    public void Delete(string id)
    {
        _metadataCollection.DeleteOne(metadata => metadata.Table.Name == id);
    }
}