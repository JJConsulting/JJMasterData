#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataDictionary.Structure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.DataDictionary.Repository;

public class SqlDataDictionaryRepository : IDataDictionaryRepository
{
    private readonly Element _masterDataElement;
    private readonly bool _enableDataDictionaryCaching;
    private readonly IEntityRepository _entityRepository;
    private readonly IMemoryCache _memoryCache;

    public SqlDataDictionaryRepository(
        IEntityRepository entityRepository, 
        IMemoryCache memoryCache,
        IOptionsSnapshot<MasterDataCoreOptions> options)
    {
        _entityRepository = entityRepository;
        _memoryCache = memoryCache;
        var mdOptions = options.Value;
        _masterDataElement = DataDictionaryStructure.GetElement(mdOptions.DataDictionaryTableSchema, mdOptions.DataDictionaryTableName);
        _enableDataDictionaryCaching = mdOptions.EnableDataDictionaryCaching;
    }

    private static readonly Guid ElementNameListCacheKey = Guid.Parse("fb545293-9793-4f00-8f2f-b961999c8202");
    public List<FormElement> GetFormElementList(bool? apiSync = null)
    {
        var parameters = GetFormElementListParameters(apiSync);

        var result = _entityRepository.GetDictionaryListResult(_masterDataElement,
            parameters, false);

        return result.Data.ConvertAll(DeserializeDictionary);
    }
    
    public async Task<List<FormElement>> GetFormElementListAsync(bool? apiSync = null)
    {
        var parameters = GetFormElementListParameters(apiSync);

        var result = await _entityRepository.GetDictionaryListResultAsync(_masterDataElement,
            parameters, false);

        return result.Data.ConvertAll(DeserializeDictionary);
    }

    private static EntityParameters GetFormElementListParameters(bool? apiSync)
    {
        var filters = new Dictionary<string, object?>();
        if (apiSync.HasValue)
            filters.Add(DataDictionaryStructure.EnableSynchronism, apiSync);

        filters[DataDictionaryStructure.Type] = "F";

        var orderBy = new OrderByData();
        orderBy.AddOrReplace(DataDictionaryStructure.Name, OrderByDirection.Asc);
        orderBy.AddOrReplace(DataDictionaryStructure.Type, OrderByDirection.Asc);
        return new EntityParameters{Filters = filters, OrderBy = orderBy};
    }

    private static FormElement DeserializeDictionary(Dictionary<string, object?> dictionary)
    {
        return JsonSerializer.Deserialize<FormElement>(dictionary[DataDictionaryStructure.Json]!.ToString()!)!;
    }

    public async ValueTask<List<string>> GetElementNameListAsync()
    {
        if (_enableDataDictionaryCaching && _memoryCache.TryGetValue(ElementNameListCacheKey, out List<string>? nameList))
        {
            return nameList!;
        }
        var filter = new Dictionary<string, object?> { { DataDictionaryStructure.Type, "F" } };

        var orderBy = new OrderByData();
        orderBy.AddOrReplace(DataDictionaryStructure.Name, OrderByDirection.Asc);
        
        var dt = await _entityRepository.GetDictionaryListResultAsync(_masterDataElement,
            new EntityParameters { Filters = filter, OrderBy = orderBy}, false);

        nameList = dt.Data.ConvertAll(row => row[DataDictionaryStructure.Name]!.ToString()!);
        
        _memoryCache.Set(ElementNameListCacheKey, nameList);
        
        return nameList;
    }

    public FormElement? GetFormElement(string elementName)
    {
        if (_enableDataDictionaryCaching && _memoryCache.TryGetValue(elementName, out FormElement? formElement))
            return formElement!.DeepCopy();
        
        var filter = new Dictionary<string, object> { { DataDictionaryStructure.Name, elementName } };

        var values =  _entityRepository.GetFields(_masterDataElement, filter);

        var model = DataDictionaryModel.FromDictionary(values);

        if (model != null)
        {
            formElement = JsonSerializer.Deserialize<FormElement>(model.Json);
            
            if(_enableDataDictionaryCaching)
                _memoryCache.Set(elementName, formElement);
            
            return formElement!.DeepCopy();
        }

        return null;
    }

    public async ValueTask<FormElement> GetFormElementAsync(string elementName)
    {
        if (_enableDataDictionaryCaching && _memoryCache.TryGetValue(elementName, out FormElement? formElement))
            return formElement!.DeepCopy();
        
        var filter = new Dictionary<string, object> { { DataDictionaryStructure.Name, elementName } };

        var values = await _entityRepository.GetFieldsAsync(_masterDataElement, filter);

        if (values.Count == 0)
            throw new JJMasterDataException($"Element {elementName} was not found.");
        
        var model = DataDictionaryModel.FromDictionary(values);
        
        formElement = JsonSerializer.Deserialize<FormElement>(model.Json);
        
        if(_enableDataDictionaryCaching)
            _memoryCache.Set(elementName, formElement);
        
        return formElement!.DeepCopy();
    }
    
    public async Task InsertOrReplaceAsync(FormElement formElement)
    {
        var values = GetFormElementDictionary(formElement);

        await _entityRepository.SetValuesAsync(_masterDataElement, values);

        if(_enableDataDictionaryCaching)
            ClearCache(formElement);
    }
    
    public async Task InsertOrReplaceAsync(IEnumerable<FormElement> formElements)
    {
        var formElementsList = formElements.ToList(); 

        var values = GetValues(formElementsList);

        await _entityRepository.SetValuesAsync(_masterDataElement, values);
    
        foreach (var formElement in formElementsList)
            ClearCache(formElement);
    }
    
    private static IEnumerable<Dictionary<string, object?>> GetValues(List<FormElement> formElements)
    {
        foreach (var formElement in formElements)
        {
            var elementDictionary = GetFormElementDictionary(formElement);
            yield return elementDictionary;
        }
    }

    public void InsertOrReplace(FormElement formElement)
    {
        var values = GetFormElementDictionary(formElement);

        _entityRepository.SetValues(_masterDataElement, values);
        
        if(_enableDataDictionaryCaching)
            ClearCache(formElement);
    }

    private static Dictionary<string, object?> GetFormElementDictionary(FormElement formElement)
    {
        if (formElement == null)
            throw new ArgumentNullException(nameof(formElement));

        if (formElement == null)
            throw new ArgumentNullException(nameof(formElement));

        if (string.IsNullOrEmpty(formElement.Name))
            throw new ArgumentNullException(nameof(formElement.Name));

        var name = formElement.Name;

        var dNow = DateTime.Now;

        var jsonForm = JsonSerializer.Serialize(formElement);

        var values = new Dictionary<string, object?>
        {
            { DataDictionaryStructure.Name, name },
            { DataDictionaryStructure.TableName, formElement.TableName },
            { DataDictionaryStructure.Info, formElement.Info },
            { DataDictionaryStructure.Type, formElement.TypeIdentifier },
            { DataDictionaryStructure.Owner, null },
            { DataDictionaryStructure.Json, jsonForm },
            { DataDictionaryStructure.EnableSynchronism, formElement.EnableSynchronism },
            { DataDictionaryStructure.LastModified, dNow }
        };

        return values;
    }

    public async Task DeleteAsync(string elementName)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentException();

        var filters = new Dictionary<string, object> { { DataDictionaryStructure.Name, elementName } };

        await _entityRepository.DeleteAsync(_masterDataElement, filters);
        
        if(_enableDataDictionaryCaching)
            _memoryCache.Remove(elementName);
    }


    public async Task<bool> ExistsAsync(string elementName)
    {
        var filter = new Dictionary<string, object> { { DataDictionaryStructure.Name, elementName } };
        var fields = await _entityRepository.GetFieldsAsync(_masterDataElement, filter);
        return fields.Count > 0;
    }

    public async Task CreateStructureIfNotExistsAsync()
    {
        if (!await _entityRepository.TableExistsAsync(_masterDataElement.Schema, _masterDataElement.Name, _masterDataElement.ConnectionId))
            await _entityRepository.CreateDataModelAsync(_masterDataElement,[]);
    }

    public async Task<ListResult<FormElementInfo>> GetFormElementInfoListAsync(DataDictionaryFilter filter,
        OrderByData orderBy, int recordsPerPage, int currentPage)
    {
        var filters = filter.ToDictionary();
        filters.Add(DataDictionaryStructure.Type, "F");

        var result = await _entityRepository.GetDictionaryListResultAsync(_masterDataElement,
            new EntityParameters
            {
                Filters = filters!, 
                OrderBy = orderBy,
                CurrentPage = currentPage,
                RecordsPerPage = recordsPerPage
            });

        var formElementInfoList = result.Data.ConvertAll(FormElementInfo.FromDictionary);

        return new ListResult<FormElementInfo>(formElementInfoList, result.TotalOfRecords);
    }

    private void ClearCache(FormElement formElement)
    {
        if (!formElement.UseReadProcedure)
            _memoryCache.Remove(formElement.Name + "_ReadScript");
        if (!formElement.UseWriteProcedure)
            _memoryCache.Remove(formElement.Name + "_WriteScript");

        
        _memoryCache.Remove(ElementNameListCacheKey);
        _memoryCache.Remove(formElement.Name);
    }
}