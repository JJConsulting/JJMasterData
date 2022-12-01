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

    public MongoDictionaryRepository(IOptions<JJMasterDataMongoDBOptions> options)
    {
        var mongoClient = new MongoClient(
            options.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            options.Value.DatabaseName);

        _metadataCollection = mongoDatabase.GetCollection<MongoDBMetadata>(
            options.Value.CollectionName);
    }

    
    public void CreateStructureIfNotExists(){}

    
    public Metadata GetMetadata(string dictionaryName)
    {
        return _metadataCollection.Find(metadata=> metadata.Table.Name == dictionaryName).FirstOrDefault();
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

    
    public DataTable GetDataTable(DataDictionaryFilter filters, string orderBy, int recordsPerPage, int currentPage, ref int totalRecords)
    {
        var bsonFilter = new BsonDocument(MapStructureFields(filters));

        IFindFluent<MongoDBMetadata, MongoDBMetadata> metadataFinder;

        if (!bsonFilter.Any())
        {
            metadataFinder = _metadataCollection.Find(_ => true);
        }
        else
        {
            metadataFinder = _metadataCollection.Find(bsonFilter);
        }

        if (!string.IsNullOrEmpty(orderBy))
        {
            var orderByMapper = MapOrderBy(orderBy);

            metadataFinder.Sort(new BsonDocument(orderByMapper.ToDictionary()));
        }
        
        if (totalRecords <= 0)
            totalRecords = (int)metadataFinder.CountDocuments();
        
        var metadataList = metadataFinder
            .Skip((currentPage - 1) * recordsPerPage)
            .Limit(recordsPerPage)
            .ToList();
        
        var values = new List<IDictionary>();

        foreach (var metadata in metadataList)
        {
            values.AddRange(DataDictionaryStructure.GetStructure(metadata, metadata.LastModified));
        }
        
        return JsonConvert.DeserializeObject<DataTable>(JsonConvert.SerializeObject(values.Where(v=>(string)v["type"]! =="F")))!;
    }
    
    
    public bool Exists(string dictionaryName)
    {
        return _metadataCollection.Find(metadata => metadata.Table.Name == dictionaryName).ToList().Count > 0;
    }

    
    public void InsertOrReplace(Metadata metadata)
    {
        var mongoDbMetadata = MongoDBMetadataMapper.FromMetadata(metadata);
        
        mongoDbMetadata.LastModified = DateTime.Now;

        _metadataCollection.ReplaceOne(
            filter: m=>metadata.Table.Name == m.Table.Name,
            options: new ReplaceOptions { IsUpsert = true },
            replacement: mongoDbMetadata);
    }

    
    public void Delete(string dictionaryName)
    {
        _metadataCollection.DeleteOne(metadata => metadata.Table.Name == dictionaryName);
    }

    private static IDictionary MapStructureFields(DataDictionaryFilter filter)
    {

        var filters = new Hashtable();

        if (filter.Name != null)
        {
            filters["Table.Name"] = filter.Name;
        }
        
        if (filter.ContainsTableName != null)
        {
            filters["Table.TableName"] = new Hashtable()
            {
                {"$in", filter.ContainsTableName}
            };
        }
        
        if (filter.LastModifiedFrom != null && filter.LastModifiedTo != null)
        {
            filters["LastModified"] = new Hashtable
            {
                {"$gt", filter.LastModifiedFrom.Value},
                {"$lt", filter.LastModifiedTo.Value}
            };
        }

        return filters;
    }
    
    private static MongoDBOrderByMapper MapOrderBy(string orderBy)
    {
        string name = orderBy.Split(" ")[0];
        string type = orderBy.Split(" ")[1];
        
        return name switch
        {
            "name" => new MongoDBOrderByMapper("Table.Name", type),
            "tablename" => new MongoDBOrderByMapper("Table.TableName", type),
            "modified" => new MongoDBOrderByMapper("LastModified", type),
            "info" => new MongoDBOrderByMapper("Table.Info", type),
            "sync" => new MongoDBOrderByMapper("Table.Sync", type),
            _ => throw new ArgumentException(orderBy)
        };
    }

}