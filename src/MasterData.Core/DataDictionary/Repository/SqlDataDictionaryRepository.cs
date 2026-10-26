
#nullable disable warnings
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
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.DataDictionary.Repository;

public class SqlDataDictionaryRepository : IDataDictionaryRepository
{
    private readonly Element _masterDataElement;
    private readonly IEntityRepository _entityRepository;

    public SqlDataDictionaryRepository(
        IEntityRepository entityRepository, 
        IOptionsSnapshot<MasterDataCoreOptions> options)
    {
        _entityRepository = entityRepository;
        
        var mdOptions = options.Value;
        _masterDataElement = DataDictionaryStructure.GetElement(mdOptions.DataDictionaryTableSchema, mdOptions.DataDictionaryTableName);
    }

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
        var filter = new Dictionary<string, object?> { { DataDictionaryStructure.Type, "F" } };

        var orderBy = new OrderByData();
        orderBy.AddOrReplace(DataDictionaryStructure.Name, OrderByDirection.Asc);
        
        var dt = await _entityRepository.GetDictionaryListResultAsync(_masterDataElement,
            new EntityParameters { Filters = filter, OrderBy = orderBy}, false);

        var nameList = dt.Data.ConvertAll(row => row[DataDictionaryStructure.Name]!.ToString()!);

        return nameList;
    }

    public FormElement? GetFormElement(string elementName)
    {
        
        var filter = new Dictionary<string, object> { { DataDictionaryStructure.Name, elementName } };

        var values =  _entityRepository.GetFields(_masterDataElement, filter);

        var model = DataDictionaryModel.FromDictionary(values);

        if (model != null)
        {
            var formElement = JsonSerializer.Deserialize<FormElement>(model.Json);

            return formElement;
        }

        return null;
    }

    public async ValueTask<FormElement> GetFormElementAsync(string elementName)
    {
        
        var filter = new Dictionary<string, object> {
        {
            DataDictionaryStructure.Name,
            elementName
        } 
        };

        var values = await _entityRepository.GetFieldsAsync(_masterDataElement, filter);

        if (values.Count == 0)
            throw new JJMasterDataException($"Element {elementName} was not found.");
        
        var model = DataDictionaryModel.FromDictionary(values);
        
        var formElement = JsonSerializer.Deserialize<FormElement>(model.Json);

        return formElement;
    }
    
    public async Task InsertOrReplaceAsync(FormElement formElement)
    {
        var values = GetFormElementDictionary(formElement);

        await _entityRepository.SetValuesAsync(_masterDataElement, values);
    }
    
    public async Task InsertOrReplaceAsync(IEnumerable<FormElement> formElements)
    {
        var formElementsList = formElements.ToList(); 

        var values = GetValues(formElementsList);

        await _entityRepository.SetValuesAsync(_masterDataElement, values);
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

}
