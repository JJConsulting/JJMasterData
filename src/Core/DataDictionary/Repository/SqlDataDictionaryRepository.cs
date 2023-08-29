#nullable enable
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Data.Extensions;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.Options;
using System;
using System.Collections.Generic;
using System.Linq;
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
            filters.Add(DataDictionaryStructure.EnableApi, apiEnabled);

        filters[DataDictionaryStructure.Name] = "F";

        var orderBy = new OrderByData();
        orderBy.AddOrReplace(DataDictionaryStructure.Name, OrderByDirection.Asc);
        orderBy.AddOrReplace(DataDictionaryStructure.Type, OrderByDirection.Asc);

        var result = await _entityRepository.GetDictionaryListAsync(MasterDataElement,
            new EntityParameters { Filters = filters, OrderBy = orderBy }, false);

        return ParseDictionaryList(result.Data);
    }

    private static IEnumerable<FormElement> ParseDictionaryList(IEnumerable<Dictionary<string, object?>> result)
    {
        foreach (var row in result)
        {
            yield return FormElementSerializer.Deserialize(row[DataDictionaryStructure.Json]!.ToString()!);
        }
    }

    public async IAsyncEnumerable<string> GetNameListAsync()
    {
        var filter = new Dictionary<string, object?> { { DataDictionaryStructure.Type, "F" } };

        var dt = await _entityRepository.GetDictionaryListAsync(MasterDataElement,
            new EntityParameters { Filters = filter }, false);
        foreach (var row in dt.Data)
        {
            yield return row[DataDictionaryStructure.Name]!.ToString()!;
        }
    }


    public async Task<FormElement?> GetMetadataAsync(string dictionaryName)
    {
        var filter = new Dictionary<string, object> { { DataDictionaryStructure.Name, dictionaryName }, {DataDictionaryStructure.Type, "F" } };

        var values = await _entityRepository.GetFieldsAsync(MasterDataElement, filter);

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
            { DataDictionaryStructure.Name, name },
            { DataDictionaryStructure.TableName, formElement.TableName },
            { DataDictionaryStructure.Info, formElement.Info },
            { DataDictionaryStructure.Type, "F" },
            { DataDictionaryStructure.Owner, null },
            { DataDictionaryStructure.Json, jsonForm },
            { DataDictionaryStructure.EnableApi, formElement.EnableApi },
            { DataDictionaryStructure.LastModified, dNow }
        };

        return values;
    }

    public async Task DeleteAsync(string dictionaryName)
    {
        if (string.IsNullOrEmpty(dictionaryName))
            throw new ArgumentException();

        var filters = new Dictionary<string, object> { { DataDictionaryStructure.Name, dictionaryName } };

        await _entityRepository.DeleteAsync(MasterDataElement, filters);
    }


    public async Task<bool> ExistsAsync(string dictionaryName)
    {
        var filter = new Dictionary<string, object> { { DataDictionaryStructure.Name, dictionaryName } };
        var fields = await _entityRepository.GetFieldsAsync(MasterDataElement, filter);
        return fields.Any();
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
        filters.Add(DataDictionaryStructure.Type, "F");

        var result = await _entityRepository.GetDictionaryListAsync(MasterDataElement,
            new EntityParameters
            {
                Filters = filters!, OrderBy = orderBy, CurrentPage = currentPage, RecordsPerPage = recordsPerPage
            });

        var formElementInfoList = result.Data.Select(FormElementInfo.FromDictionary).ToList();

        return new ListResult<FormElementInfo>(formElementInfoList, result.TotalOfRecords);
    }
}