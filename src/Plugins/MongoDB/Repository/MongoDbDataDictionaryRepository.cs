using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.MongoDB.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;

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

    public Task CreateStructureIfNotExistsAsync() => Task.CompletedTask;


    public async Task<FormElement> GetMetadataAsync(string dictionaryName)
    {
        var formElementQuery = await _formElementCollection.FindAsync(formElement => formElement.FormElement.Name == dictionaryName);

        return (await formElementQuery.FirstAsync()).FormElement;
    }
    public async Task<IEnumerable<FormElement>> GetMetadataListAsync(bool? sync = null)
    {
        var formElements = await _formElementCollection.FindAsync(_ => true);

        return (await formElements.ToListAsync()).Select(f=>f.FormElement);
    }
    

    public async IAsyncEnumerable<string> GetNameListAsync()
    {
        var formElements = await _formElementCollection.FindAsync(_ => true);
        
        foreach (var formElement in await formElements.ToListAsync())
        {
            yield return formElement.FormElement.Name;
        }
    }
    
    public async Task<ListResult<FormElementInfo>> GetFormElementInfoListAsync(DataDictionaryFilter filters, OrderByData orderBy, int recordsPerPage, int currentPage)
    {
        int totalRecords = 0;
        
        var query = CreateInfoQuery(filters, orderBy, recordsPerPage, currentPage, ref totalRecords);

        var list = await query.ToListAsync();
        
        return new ListResult<FormElementInfo>(list.Select(formElement => new FormElementInfo(formElement.FormElement, formElement.LastModified)).ToList(),totalRecords);
    }
    
    
    private IFindFluent<MongoDBFormElement, MongoDBFormElement> CreateInfoQuery(DataDictionaryFilter filters, OrderByData orderBy, int recordsPerPage, int currentPage,
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

        if (!string.IsNullOrEmpty(orderBy.ToQueryParameter()))
        {
            var orderByMapper = MapOrderBy(orderBy.ToQueryParameter()!);

            formElementFinder.Sort(new BsonDocument(orderByMapper.ToDictionary()));
        }

        if (totalRecords <= 0)
            totalRecords = (int)formElementFinder.CountDocuments();

        var query = formElementFinder
            .Skip((currentPage - 1) * recordsPerPage)
            .Limit(recordsPerPage);
        return query;
    }

    public async Task<bool> ExistsAsync(string dictionaryName)
    {
        var finder = await _formElementCollection.FindAsync(formElement => formElement.FormElement.Name == dictionaryName);
        return (await finder.ToListAsync()).Count > 0;
    }
    
    public async Task InsertOrReplaceAsync(FormElement formElement)
    {
        await _formElementCollection.ReplaceOneAsync(
            filter: m=>formElement.Name == m.FormElement.Name,
            options: new ReplaceOptions { IsUpsert = true },
            replacement: new MongoDBFormElement(formElement));
    }


    public async Task DeleteAsync(string dictionaryName)
    {
        await _formElementCollection.DeleteOneAsync(formElement => formElement.FormElement.Name == dictionaryName);
    }

    private static Dictionary<string,object> MapStructureFields(DataDictionaryFilter filter)
    {

        var filters = new Dictionary<string,object>();

        if (filter.Name != null)
        {
            filters["FormElement.Name"] = new Dictionary<string,object>
            {
                {"$regex", filter.Name}
            };
        }
        
        if (filter.ContainsTableName != null)
        {
            filters["FormElement.TableName"] = new Dictionary<string,object>
            {
                {"$in", filter.ContainsTableName}
            };
        }
        
        if (filter is { LastModifiedFrom: not null, LastModifiedTo: not null })
        {
            filters["Modified"] = new Dictionary<string,object>
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
            DataDictionaryStructure.Name => new MongoDBOrderByMapper("FormElement.Table.Name", type),
            DataDictionaryStructure.TableName => new MongoDBOrderByMapper("FormElement.Table.TableName", type),
            DataDictionaryStructure.LastModified => new MongoDBOrderByMapper("Modified", type),
            DataDictionaryStructure.Info  => new MongoDBOrderByMapper("FormElement.Table.Info", type),
            DataDictionaryStructure.EnableApi   => new MongoDBOrderByMapper("FormElement.Table.Sync", type),
            _ => throw new ArgumentException(orderBy)
        };
    }

}