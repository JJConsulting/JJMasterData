#nullable enable
using JJMasterData.Commons.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataDictionary.Structure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.DataDictionary.Repository;

public class SqlDataDictionaryRepository(
    IEntityRepository entityRepository, 
    IMemoryCache memoryCache,
    IOptionsSnapshot<MasterDataCoreOptions> options)
    : IDataDictionaryRepository
{
    private readonly Element _masterDataElement  = DataDictionaryStructure.GetElement(options.Value.DataDictionaryTableName);
    private readonly bool _enableDataDictionaryCaching  = options.Value.EnableDataDictionaryCaching;
    
    public List<FormElement> GetFormElementList(bool? apiSync = null)
    {
        var parameters = GetFormElementListParameters(apiSync);

        var result = entityRepository.GetDictionaryListResult(_masterDataElement,
            parameters, false);

        return ParseDictionaryList(result.Data).ToList();
    }
    
    public async Task<List<FormElement>> GetFormElementListAsync(bool? apiSync = null)
    {
        var parameters = GetFormElementListParameters(apiSync);

        var result = await entityRepository.GetDictionaryListResultAsync(_masterDataElement,
            parameters, false);

        return ParseDictionaryList(result.Data).ToList();
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

    private static IEnumerable<FormElement> ParseDictionaryList(IEnumerable<Dictionary<string, object?>> result)
    {
        foreach (var row in result)
        {
            yield return FormElementSerializer.Deserialize(row[DataDictionaryStructure.Json]!.ToString()!);
        }
    }

    public async Task<List<string>> GetNameListAsync()
    {
        var filter = new Dictionary<string, object?> { { DataDictionaryStructure.Type, "F" } };

        var dt = await entityRepository.GetDictionaryListResultAsync(_masterDataElement,
            new EntityParameters { Filters = filter }, false);

        return dt.Data.Select(row => row[DataDictionaryStructure.Name]!.ToString()!).ToList();
    }

    public FormElement? GetFormElement(string elementName)
    {
        if (_enableDataDictionaryCaching && memoryCache.TryGetValue(elementName, out FormElement? formElement))
            return formElement!.DeepCopy();
        
        var filter = new Dictionary<string, object> { { DataDictionaryStructure.Name, elementName } };

        var values =  entityRepository.GetFields(_masterDataElement, filter);

        var model = values.ToModel<DataDictionaryModel>();

        if (model != null)
        {
            formElement = FormElementSerializer.Deserialize(model.Json);
            
            if(_enableDataDictionaryCaching)
                memoryCache.Set(elementName, formElement);
            
            return formElement.DeepCopy();
        }

        return null;
    }

    public async ValueTask<FormElement?> GetFormElementAsync(string elementName)
    {
        if (_enableDataDictionaryCaching && memoryCache.TryGetValue(elementName, out FormElement? formElement))
            return formElement!.DeepCopy();
        
        var filter = new Dictionary<string, object> { { DataDictionaryStructure.Name, elementName } };

        var values = await entityRepository.GetFieldsAsync(_masterDataElement, filter);

        var model = values.ToModel<DataDictionaryModel>();
        
        if (model != null)
        {
            formElement = FormElementSerializer.Deserialize(model.Json);
            
            if(_enableDataDictionaryCaching)
                memoryCache.Set(elementName, formElement);
            
            return formElement.DeepCopy();
        }

        return null;
    }


    public async Task InsertOrReplaceAsync(FormElement formElement)
    {
        var values = GetFormElementDictionary(formElement);

        await entityRepository.SetValuesAsync(_masterDataElement, values);

        if(_enableDataDictionaryCaching)
            memoryCache.Remove(formElement.Name);
    }

    public void InsertOrReplace(FormElement formElement)
    {
        var values = GetFormElementDictionary(formElement);

        entityRepository.SetValues(_masterDataElement, values);
        
        if(_enableDataDictionaryCaching)
            memoryCache.Remove(formElement.Name);
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

        var jsonForm = FormElementSerializer.Serialize(formElement);

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

        await entityRepository.DeleteAsync(_masterDataElement, filters);
        
        if(_enableDataDictionaryCaching)
            memoryCache.Remove(elementName);
    }


    public async Task<bool> ExistsAsync(string elementName)
    {
        var filter = new Dictionary<string, object> { { DataDictionaryStructure.Name, elementName } };
        var fields = await entityRepository.GetFieldsAsync(_masterDataElement, filter);
        return fields.Any();
    }

    public async Task CreateStructureIfNotExistsAsync()
    {
        if (!await entityRepository.TableExistsAsync(_masterDataElement.Name, _masterDataElement.ConnectionId))
            await entityRepository.CreateDataModelAsync(_masterDataElement,[]);
            
    }

    public async Task<ListResult<FormElementInfo>> GetFormElementInfoListAsync(DataDictionaryFilter filter,
        OrderByData orderBy, int recordsPerPage, int currentPage)
    {
        var filters = filter.ToDictionary();
        filters.Add(DataDictionaryStructure.Type, "F");

        var result = await entityRepository.GetDictionaryListResultAsync(_masterDataElement,
            new EntityParameters
            {
                Filters = filters!, OrderBy = orderBy, CurrentPage = currentPage, RecordsPerPage = recordsPerPage
            });

        var formElementInfoList = result.Data.Select(FormElementInfo.FromDictionary).ToList();

        return new ListResult<FormElementInfo>(formElementInfoList, result.TotalOfRecords);
    }
}