using System.Collections;
using System.Data;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.MongoDB.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace JJMasterData.MongoDB.Repository;

public class MongoDictionaryRepository : IDictionaryRepository
{
    private readonly IMongoCollection<MongoDBMetadata> _metadataCollection;

    public bool IsSql => false;
    
    public MongoDictionaryRepository(IOptions<JJMasterDataMongoDBOptions> options)
    {
        var mongoClient = new MongoClient(
            options.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            options.Value.DatabaseName);

        _metadataCollection = mongoDatabase.GetCollection<MongoDBMetadata>(
            options.Value.CollectionName);
    }
    public void CreateStructure()
    {
        throw new InvalidOperationException(
            "MongoDB don't need initial setup. It automatically handle database and table creation at runtime.");
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
        var bsonFilter = new BsonDocument(MapStructureFilters(filters));

        IFindFluent<MongoDBMetadata, MongoDBMetadata> metadataFinder;

        if (!bsonFilter.Any())
        {
            metadataFinder = _metadataCollection.Find(_ => true);
        }
        else
        {
            metadataFinder = _metadataCollection.Find(bsonFilter);
        }

        var metadataList = metadataFinder.ToList();
            // .Sort(orderby)
            // .Skip((pag - 1) * regperpage)
            // .Limit(regperpage)
            // .ToList();
        
        tot = metadataList.Count;

        var values = new List<IDictionary>();

        foreach (var metadata in metadataList)
        {
            values.AddRange(MetadataStructure.GetStructure(metadata));
        }
        
        return JsonConvert.DeserializeObject<DataTable>(JsonConvert.SerializeObject(values.Where(v=>(string)v["type"]=="F")))!;
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

    private IDictionary MapStructureFilters(IDictionary structureFilters)
    {

        var filters = new Hashtable();

        if (structureFilters["namefilter"] != null)
        {
            filters["Table.Name"] = structureFilters["namefilter"];
        }
        
        if (structureFilters["tablename"] != null)
        {
            filters["Table.TableName"] = structureFilters["tablename"];
        }

        return filters;
    }
}