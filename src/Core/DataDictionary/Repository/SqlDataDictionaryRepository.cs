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
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.DataDictionary.Repository;

public class SqlDataDictionaryRepository : IDataDictionaryRepository
{
    private readonly IEntityRepository _entityRepository;
    internal Element MasterDataElement { get;  }

    public SqlDataDictionaryRepository(IEntityRepository entityRepository, IOptions<JJMasterDataCoreOptions> options)
    {
        _entityRepository = entityRepository;
        MasterDataElement = DataDictionaryStructure.GetElement(options.Value.DataDictionaryTableName);
    }

    ///<inheritdoc cref="IDataDictionaryRepository.GetMetadataList"/>
    public IEnumerable<FormElement> GetMetadataList(bool? sync = null)
    {
        var list = new List<FormElement>();

        var filter = new Hashtable();
        if (sync.HasValue)
            filter.Add("sync", (bool)sync ? "1" : "0");

        const string orderBy = "name, type";
        const string currentName = "";
        var tot = 1;
        var dt = _entityRepository.GetDataTable(MasterDataElement, filter, orderBy, 10000, 1, ref tot);

        return ParseDataTable(dt, currentName, list);
    }

    public async Task<IEnumerable<FormElement>> GetMetadataListAsync(bool? sync = null)
    {
        var list = new List<FormElement>();

        var filter = new Hashtable();
        if (sync.HasValue)
            filter.Add("sync", (bool)sync ? "1" : "0");

        const string orderBy = "name, type";
        const string currentName = "";
        var result = await _entityRepository.GetDataTableAsync(MasterDataElement, filter, orderBy, 10000, 1, false);

        return ParseDataTable(result.Data, currentName, list);
    }

    private static IEnumerable<FormElement> ParseDataTable(DataTable dt, string currentName, List<FormElement> list)
    {
        FormElement currentParser = null;
        foreach (DataRow row in dt.Rows)
        {
            var name = row["name"].ToString();
            if (!currentName.Equals(name))
            {
                currentName = name;
                list.Add(new FormElement());
            }

            var json = row["json"].ToString();
            currentParser = row["type"].ToString()! switch
            {
                "F" => FormElementSerializer.Deserialize(json),
                _ => currentParser
            };
        }

        return list;
    }

    ///<inheritdoc cref="IDataDictionaryRepository.GetNameList"/>
    public IEnumerable<string> GetNameList()
    {
        var totalRecords = 10000;
        var filter = new Dictionary<string, object> { { "type", "F" } };

        var dt = _entityRepository.GetDataTable(MasterDataElement, filter, null, totalRecords, 1, ref totalRecords);
        foreach (DataRow row in dt.Rows)
        {
            yield return row["name"].ToString();
        }
    }

    public async IAsyncEnumerable<string> GetNameListAsync()
    {
        const int totalRecords = 10000;
        var filter = new Dictionary<string, object>{ { "type", "F" } };

        var dt = await _entityRepository.GetDataTableAsync(MasterDataElement, filter, null, totalRecords, 1, false);
        foreach (DataRow row in dt.Data.Rows)
        {
            yield return row["name"].ToString();
        }
    }

    ///<inheritdoc cref="IDataDictionaryRepository.GetMetadata"/>
    public FormElement GetMetadata(string dictionaryName)
    {
        var filter = new Dictionary<string, object> { { "name", dictionaryName },{"type", "F"} };

        var values = _entityRepository.GetFields(MasterDataElement, filter);
        
        var model = values?.ToModel<DataDictionaryModel>();
        
        return FormElementSerializer.Deserialize(model?.Json);
    }

    public async Task<FormElement> GetMetadataAsync(string dictionaryName)
    {
        var filter = new Dictionary<string, object> { { "name", dictionaryName },{"type", "F"} };

        var values = await _entityRepository.GetDictionaryAsync(MasterDataElement, filter);
        
        var model = values?.ToModel<DataDictionaryModel>();
        
        return model != null ? FormElementSerializer.Deserialize(model.Json) : null;
    }

    ///<inheritdoc cref="IDataDictionaryRepository.InsertOrReplace"/>
    public void InsertOrReplace(FormElement metadata)
    {
        var values = GetFormElementHashtable(metadata);

        _entityRepository.SetValues(MasterDataElement, values);
    }

    public async Task InsertOrReplaceAsync(FormElement formElement)
    {
        var values = GetFormElementHashtable(formElement);

        await _entityRepository.SetValuesAsync(MasterDataElement, values);
    }

    private static Hashtable GetFormElementHashtable(FormElement formElement)
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

        var values = new Hashtable
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

    ///<inheritdoc cref="IDataDictionaryRepository.Delete"/>
    public void Delete(string dictionaryName)
    {
        if (string.IsNullOrEmpty(dictionaryName))
            throw new ArgumentException();

        var filters = new Hashtable { { "name", dictionaryName } };
        //TODO: Lucio p/ que isso? era doh remover diretor e validar se qtd no delete é maior que um
        var dataTable = _entityRepository.GetDataTable(MasterDataElement, filters);
        if (dataTable.Rows.Count == 0)
            throw new KeyNotFoundException($"Dictionary {dictionaryName} not found");

        foreach (DataRow row in dataTable.Rows)
        {
            var delFilter = new Hashtable
            {
                { "name", dictionaryName },
                { "type", row["type"].ToString() }
            };
            _entityRepository.Delete(MasterDataElement, delFilter);
        }
    }

    public async Task DeleteAsync(string dictionaryName)
    {
        if (string.IsNullOrEmpty(dictionaryName))
            throw new ArgumentException();

        var filters = new Dictionary<string, object> { { "name", dictionaryName } };

        var dataTable = await _entityRepository.GetDataTableAsync(MasterDataElement, filters);
        if (dataTable.Rows.Count == 0)
            throw new KeyNotFoundException($"Dictionary {dictionaryName} not found");

        foreach (DataRow row in dataTable.Rows)
        {
            var delFilter = new Dictionary<string, object>()
            {
                { "name", dictionaryName },
                { "type", row["type"].ToString() }
            };
            await _entityRepository.DeleteAsync(MasterDataElement, delFilter);
        }
    }

    ///<inheritdoc cref="IDataDictionaryRepository.Exists"/>
    public bool Exists(string elementName)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName));

        var filter = new Hashtable { { "name", elementName } };
        var count = _entityRepository.GetCount(MasterDataElement, filter);
        return count > 0;
    }

    public async Task<bool> ExistsAsync(string dictionaryName)
    {
        if (string.IsNullOrEmpty(dictionaryName))
            throw new ArgumentNullException(nameof(dictionaryName));

        var filter = new Hashtable { { "name", dictionaryName } };
        var count = await _entityRepository.GetCountAsync(MasterDataElement, filter);
        return count > 0;
    }

    ///<inheritdoc cref="IDataDictionaryRepository.CreateStructureIfNotExists"/>
    public void CreateStructureIfNotExists()
    {
        if (!_entityRepository.TableExists(MasterDataElement.Name))
            _entityRepository.CreateDataModel(MasterDataElement);
    }

    public async Task CreateStructureIfNotExistsAsync()
    {
        if (!await _entityRepository.TableExistsAsync(MasterDataElement.Name))
            await _entityRepository.CreateDataModelAsync(MasterDataElement);
    }

    ///<inheritdoc cref="IDataDictionaryRepository.GetMetadataInfoList"/>
    public IEnumerable<FormElementInfo> GetMetadataInfoList(DataDictionaryFilter filter, string orderBy,
        int recordsPerPage, int currentPage, ref int totalRecords)
    {
        var filters = (IDictionary)filter.ToDictionary();
        filters.Add("type", "F");

        var dt = _entityRepository.GetDataTable(MasterDataElement, filters, orderBy, recordsPerPage, currentPage,
            ref totalRecords);
        return dt.ToModelList<FormElementInfo>();
    }

    public async Task<EntityResult<FormElementInfo>> GetFormElementInfoListAsync(DataDictionaryFilter filter,
        string orderBy, int recordsPerPage, int currentPage)
    {
        
        var filters = (IDictionary)filter.ToDictionary();
        filters.Add("type","F");

        //TODO: Lucio
        //var dt = _entityRepository.GetDataTable(MasterDataElement, filters, orderBy, recordsPerPage, currentPage, ref totalRecords); 
        //return dt.ToModelList<FormElementInfo>();
        
        

        var result = await _entityRepository.GetDictionaryListAsync(MasterDataElement, filters, orderBy, recordsPerPage,
            currentPage);

        var formElementInfoList = new List<FormElementInfo>();

        foreach (var element in result.Data)
        {
            var info = new FormElementInfo
            {
                Info = (string)element["info"],
                Modified = (DateTime)element["modified"],
                Name = (string)element["name"],
                Sync = (string)element["sync"],
                TableName = (string)element["tablename"]
            };
            
            formElementInfoList.Add(info);
        }

        return new EntityResult<FormElementInfo>(formElementInfoList, result.TotalOfRecords);
    }
}