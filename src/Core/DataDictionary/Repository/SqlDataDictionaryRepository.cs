#nullable enable
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Data.Extensions;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Repository;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.DataDictionary.Repository;

public class SqlDataDictionaryRepository : IDataDictionaryRepository
{
    private readonly IEntityRepository _entityRepository;
    internal Element MasterDataElement { get; }

    public SqlDataDictionaryRepository(IEntityRepository entityRepository, IOptions<JJMasterDataCoreOptions> options)
    {
        _entityRepository = entityRepository;
        MasterDataElement = DataDictionaryStructure.GetElement(options.Value.DataDictionaryTableName);
    }

    public async Task<IEnumerable<FormElement>> GetMetadataListAsync(bool? apiEnabled = null)
    {
        var filters = new Dictionary<string, object?>();
        if (apiEnabled.HasValue)
            filters.Add("sync", (bool)apiEnabled ? "1" : "0");

        filters["type"] = "F";

        var orderBy = new OrderByData();
        orderBy.AddOrReplace("name", OrderByDirection.Asc);
        orderBy.AddOrReplace("type", OrderByDirection.Asc);

        var result = await _entityRepository.GetDictionaryListAsync(MasterDataElement,
            new EntityParameters() { Filters = filters, OrderBy = orderBy }, false);

        return ParseDictionaryList(result.Data);
    }

    private static IEnumerable<FormElement> ParseDictionaryList(IEnumerable<Dictionary<string, object?>> result)
    {
        foreach (var row in result)
        {
            yield return FormElementSerializer.Deserialize(row["json"]!.ToString());
        }
    }

    public async IAsyncEnumerable<string> GetNameListAsync()
    {
        var filter = new Dictionary<string, object?> { { "type", "F" } };

        var dt = await _entityRepository.GetDictionaryListAsync(MasterDataElement,
            new EntityParameters() { Filters = filter }, false);
        foreach (var row in dt.Data)
        {
            yield return row["name"]!.ToString();
        }
    }


    public async Task<FormElement?> GetMetadataAsync(string dictionaryName)
    {
        var filter = new Dictionary<string, object> { { "name", dictionaryName }, { "type", "F" } };

        var values = await _entityRepository.GetDictionaryAsync(MasterDataElement, filter);

        var model = values.ToModel<DataDictionaryModel>();

        return model != null ? FormElementSerializer.Deserialize(model.Json) : null;
    }


    public async Task InsertOrReplaceAsync(FormElement formElement)
    {
        var values = GetFormElementDictionary(formElement);

        await _entityRepository.SetValuesAsync(MasterDataElement, values);
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
            { "name", name },
            { "tablename", formElement.TableName },
            { "info", formElement.Info },
            { "type", "F" },
            { "owner", null },
            { "json", jsonForm },
            { "sync", formElement.Sync ? "1" : "0" },
            { "modified", dNow }
        };

        return values;
    }

    public async Task DeleteAsync(string dictionaryName)
    {
        if (string.IsNullOrEmpty(dictionaryName))
            throw new ArgumentException();

        var filters = new Dictionary<string, object> { { "name", dictionaryName } };
        
        await _entityRepository.DeleteAsync(MasterDataElement, filters);
    }


    public async Task<bool> ExistsAsync(string dictionaryName)
    {
        if (string.IsNullOrEmpty(dictionaryName))
            throw new ArgumentNullException(nameof(dictionaryName));

        var filter = new Dictionary<string, object?> { { "name", dictionaryName } };
        var count = await _entityRepository.GetCountAsync(MasterDataElement, filter);
        return count > 0;
    }


    public async Task CreateStructureIfNotExistsAsync()
    {
        if (!await _entityRepository.TableExistsAsync(MasterDataElement.Name))
            await _entityRepository.CreateDataModelAsync(MasterDataElement);
    }

    public async Task<ListResult<FormElementInfo>> GetFormElementInfoListAsync(DataDictionaryFilter filter,
        OrderByData orderBy, int recordsPerPage, int currentPage)
    {
        var filters = filter.ToDictionary();
        filters.Add("type", "F");

        var result = await _entityRepository.GetDictionaryListAsync(MasterDataElement,
            new EntityParameters()
            {
                Filters = filters, OrderBy = orderBy, CurrentPage = currentPage, RecordsPerPage = recordsPerPage
            });

        var formElementInfoList = new List<FormElementInfo>();

        foreach (var element in result.Data)
        {
            var info = new FormElementInfo
            {
                Info = (string)element["info"]!,
                Modified = (DateTime)element["modified"]!,
                Name = (string)element["name"]!,
                Sync = (string)element["sync"]!,
                TableName = (string)element["tablename"]!
            };

            formElementInfoList.Add(info);
        }

        return new ListResult<FormElementInfo>(formElementInfoList, result.TotalOfRecords);
    }
}