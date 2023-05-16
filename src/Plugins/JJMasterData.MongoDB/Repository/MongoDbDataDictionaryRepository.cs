using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.MongoDB.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;

namespace JJMasterData.MongoDB.Repository;

public class MongoDbDataDictionaryRepository : IDataDictionaryRepository
{
    private readonly IMongoCollection<MongoDBFormElement> _formElementCollection;

    public MongoDbDataDictionaryRepository(IOptions<JJMasterDataMongoDBOptions> options)
    {
        var mongoClient = new MongoClient(
            options.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            options.Value.DatabaseName);

        _formElementCollection = mongoDatabase.GetCollection<MongoDBFormElement>(
            options.Value.CollectionName);
    }

    ///<inheritdoc cref="IDataDictionaryRepository.CreateStructureIfNotExists"/>
    public void CreateStructureIfNotExists(){}

    public Task CreateStructureIfNotExistsAsync() => Task.CompletedTask;

    ///<inheritdoc cref="IDataDictionaryRepository.GetMetadata"/>
    public FormElement GetMetadata(string dictionaryName)
    {
        var metadata = _formElementCollection.Find(metadata => metadata.Name == dictionaryName).FirstOrDefault();

        return metadata;
    }

    public async Task<FormElement> GetMetadataAsync(string dictionaryName)
    {
        var metadata = await _formElementCollection.FindAsync(metadata => metadata.Name == dictionaryName);

        return metadata.FirstOrDefault();
    }

    ///<inheritdoc cref="IDataDictionaryRepository.GetMetadataList"/>
    public IEnumerable<FormElement> GetMetadataList(bool? sync)
    {
        return _formElementCollection.Find(_ => true).ToList();
    }

    public async Task<IEnumerable<FormElement>> GetMetadataListAsync(bool? sync = null)
    {
        var formElements = await _formElementCollection.FindAsync(_ => true);

        return await formElements.ToListAsync();
    }

    ///<inheritdoc cref="IDataDictionaryRepository.GetNameList"/>
    public IEnumerable<string> GetNameList()
    {
        return _formElementCollection.Find(_ => true).ToList().Select(metadata => metadata.Name);
    }

    public async IAsyncEnumerable<string> GetNameListAsync()
    {
        var formElements = await _formElementCollection.FindAsync(_ => true);
        
        foreach (var formElement in await formElements.ToListAsync())
        {
            yield return formElement.Name;
        }
    }

    ///<inheritdoc cref="IDataDictionaryRepository.GetMetadataInfoList"/>
    public  IEnumerable<FormElementInfo> GetMetadataInfoList(DataDictionaryFilter filters, string orderBy, int recordsPerPage, int currentPage, ref int totalRecords)
    {
        var query = CreateInfoQuery(filters, orderBy, recordsPerPage, currentPage, ref totalRecords);

        return query.ToList().Select(metadata => new FormElementInfo(metadata, metadata.LastModified)).ToList();
    }



    public async Task<IEnumerable<FormElementInfo>> GetMetadataInfoListAsync(DataDictionaryFilter filters, string orderBy, int recordsPerPage, int currentPage,
        int totalRecords)
    {
        var query = CreateInfoQuery(filters, orderBy, recordsPerPage, currentPage, ref totalRecords);

        var list = await query.ToListAsync();
        
        return list.Select(metadata => new FormElementInfo(metadata, metadata.LastModified)).ToList();
    }
    
    private IFindFluent<MongoDBFormElement, MongoDBFormElement> CreateInfoQuery(DataDictionaryFilter filters, string orderBy, int recordsPerPage, int currentPage,
        ref int totalRecords)
    {
        var bsonFilter = new BsonDocument(MapStructureFields(filters));

        IFindFluent<MongoDBFormElement, MongoDBFormElement> formElementFinder;

        if (!bsonFilter.Any())
        {
            formElementFinder = _formElementCollection.Find(_ => true);
        }
        else
        {
            formElementFinder = _formElementCollection.Find(bsonFilter);
        }

        if (!string.IsNullOrEmpty(orderBy))
        {
            var orderByMapper = MapOrderBy(orderBy);

            formElementFinder.Sort(new BsonDocument(orderByMapper.ToDictionary()));
        }

        if (totalRecords <= 0)
            totalRecords = (int)formElementFinder.CountDocuments();

        var query = formElementFinder
            .Skip((currentPage - 1) * recordsPerPage)
            .Limit(recordsPerPage);
        return query;
    }

    ///<inheritdoc cref="IDataDictionaryRepository.Exists"/>
    public bool Exists(string dictionaryName)
    {
        return _formElementCollection.Find(formElement => formElement.Name == dictionaryName).ToList().Count > 0;
    }

    public async Task<bool> ExistsAsync(string dictionaryName)
    {
        var finder = await _formElementCollection.FindAsync(formElement => formElement.Name == dictionaryName);
        return (await finder.ToListAsync()).Count > 0;
    }

    ///<inheritdoc cref="IDataDictionaryRepository.InsertOrReplace"/>
    public void InsertOrReplace(FormElement formElement)
    {
        var mongoFormElement = MongoDBFormElementMapper.FromFormElement(formElement);

        _formElementCollection.ReplaceOne(
            filter: m=>formElement.Name == m.Name,
            options: new ReplaceOptions { IsUpsert = true },
            replacement: mongoFormElement);
    }

    public async Task InsertOrReplaceAsync(FormElement formElement)
    {
        var mongoFormElement = MongoDBFormElementMapper.FromFormElement(formElement);

        await _formElementCollection.ReplaceOneAsync(
            filter: m=>formElement.Name == m.Name,
            options: new ReplaceOptions { IsUpsert = true },
            replacement: mongoFormElement);
    }

    ///<inheritdoc cref="IDataDictionaryRepository.Delete"/>
    public void Delete(string dictionaryName)
    {
        _formElementCollection.DeleteOne(metadata => metadata.Name == dictionaryName);
    }

    public async Task DeleteAsync(string dictionaryName)
    {
        await _formElementCollection.DeleteOneAsync(metadata => metadata.Name == dictionaryName);
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
        
        if (filter is { LastModifiedFrom: not null, LastModifiedTo: not null })
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