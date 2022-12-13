using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.MongoDB.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using System.Collections;
using System.Data;
using JJMasterData.Commons.Extensions;

namespace JJMasterData.MongoDB.Repository;

public class MongoDbDataDictionaryRepository : IDataDictionaryRepository
{
    private readonly IMongoCollection<MongoDBMetadata> _metadataCollection;

    public MongoDbDataDictionaryRepository(IOptions<JJMasterDataMongoDBOptions> options)
    {
        var mongoClient = new MongoClient(
            options.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            options.Value.DatabaseName);

        _metadataCollection = mongoDatabase.GetCollection<MongoDBMetadata>(
            options.Value.CollectionName);
    }

    ///<inheritdoc cref="IDataDictionaryRepository.CreateStructureIfNotExists"/>
    public void CreateStructureIfNotExists(){}

    ///<inheritdoc cref="IDataDictionaryRepository.GetMetadata"/>
    public Metadata GetMetadata(string dictionaryName)
    {
        var metadata = _metadataCollection.Find(metadata => metadata.Table.Name == dictionaryName).FirstOrDefault();
        
        DataDictionaryStructure.ApplyCompatibility(metadata, dictionaryName);

        return metadata;
    }

    ///<inheritdoc cref="IDataDictionaryRepository.GetMetadataList"/>
    public IEnumerable<Metadata> GetMetadataList(bool? sync)
    {
        var dbMetadataCollection =  _metadataCollection.Find(_ => true).ToList();

        return dbMetadataCollection.Select(MongoDBMetadataMapper.FromMongoDBMetadata).ToList();
    }

    ///<inheritdoc cref="IDataDictionaryRepository.GetNameList"/>
    public IEnumerable<string> GetNameList()
    {
        return _metadataCollection.Find(_ => true).ToList().Select(metadata => metadata.Table.Name).ToList();
    }

    ///<inheritdoc cref="IDataDictionaryRepository.GetDataTable"/>
    public  IEnumerable<MetadataInfo>  GetDataTable(DataDictionaryFilter filters, string orderBy, int recordsPerPage, int currentPage, ref int totalRecords)
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

        return metadataList.Select(metadata => new MetadataInfo(metadata, metadata.LastModified)).ToList();
    }
    
    ///<inheritdoc cref="IDataDictionaryRepository.Exists"/>
    public bool Exists(string dictionaryName)
    {
        return _metadataCollection.Find(metadata => metadata.Table.Name == dictionaryName).ToList().Count > 0;
    }

    ///<inheritdoc cref="IDataDictionaryRepository.InsertOrReplace"/>
    public void InsertOrReplace(Metadata metadata)
    {
        var mongoDbMetadata = MongoDBMetadataMapper.FromMetadata(metadata);
        
        mongoDbMetadata.LastModified = DateTime.Now;

        _metadataCollection.ReplaceOne(
            filter: m=>metadata.Table.Name == m.Table.Name,
            options: new ReplaceOptions { IsUpsert = true },
            replacement: mongoDbMetadata);
    }

    ///<inheritdoc cref="IDataDictionaryRepository.Delete"/>
    public void Delete(string dictionaryName)
    {
        _metadataCollection.DeleteOne(metadata => metadata.Table.Name == dictionaryName);
    }

    private static IDictionary MapStructureFields(DataDictionaryFilter filter)
    {

        var filters = new Hashtable();

        if (filter.Name != null)
        {
            filters["Table.Name"] = new Hashtable
            {
                {"$regex", filter.Name}
            };
        }
        
        if (filter.ContainsTableName != null)
        {
            filters["Table.TableName"] = new Hashtable
            {
                {"$in", filter.ContainsTableName}
            };
        }
        
        if (filter is { LastModifiedFrom: { }, LastModifiedTo: { } })
        {
            filters["Modified"] = new Hashtable
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
            "modified" => new MongoDBOrderByMapper("Modified", type),
            "info" => new MongoDBOrderByMapper("Table.Info", type),
            "sync" => new MongoDBOrderByMapper("Table.Sync", type),
            _ => throw new ArgumentException(orderBy)
        };
    }

}