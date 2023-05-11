using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Security.Policy;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.Options;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.DataDictionary.Repository;

public class SqlDataDictionaryRepository : IDataDictionaryRepository
{
    private readonly IEntityRepository _entityRepository;
    private readonly IOptions<JJMasterDataCoreOptions> _options;
    private Element _masterDataElement;

    internal Element MasterDataElement
    {
        get
        {
            if (_masterDataElement == null)
            {
                string tableName = _options.Value.DataDictionaryTableName;
                _masterDataElement = DataDictionaryStructure.GetElement(tableName);
            }
            return _masterDataElement;
        }
    }
 
    public SqlDataDictionaryRepository(IEntityRepository entityRepository, IOptions<JJMasterDataCoreOptions> options)
    {
        _entityRepository = entityRepository;
        _options = options;
    }

    public Task<FormElement> GetMetadataAsync(string dictionaryName)
    {
        throw new NotImplementedException();
    }

    ///<inheritdoc cref="IDataDictionaryRepository.GetMetadataList"/>
    public IEnumerable<FormElement> GetMetadataList(bool? sync = null)
    {
        var list = new List<FormElement>();

        var filter = new Hashtable();
        if (sync.HasValue)
            filter.Add("sync", (bool)sync ? "1" : "0");

        const string orderBy = "name, type";
        string currentName = "";
        int tot = 1;
        var dt = _entityRepository.GetDataTable(MasterDataElement, filter, orderBy, 10000, 1, ref tot);
        
        return ParseDataTable(dt, currentName, list);
    }

    public Task<IEnumerable<FormElement>> GetMetadataListAsync(bool? sync = null)
    {
        throw new NotImplementedException();
    }

    private static IEnumerable<FormElement> ParseDataTable(DataTable dt, string currentName, List<FormElement> list)
    {
        FormElement currentParser = null;
        foreach (DataRow row in dt.Rows)
        {
            string name = row["name"].ToString();
            if (!currentName.Equals(name))
            {
                currentName = name;
                list.Add(new FormElement());
            }

            string json = row["json"].ToString();
            currentParser = row["type"].ToString()! switch
            {
                "F" => JsonConvert.DeserializeObject<FormElement>(json),
                _ => currentParser
            };
        }

        return list;
    }

    ///<inheritdoc cref="IDataDictionaryRepository.GetNameList"/>
    public IEnumerable<string> GetNameList()
    {
        var totalRecords = 10000;
        var filter = new Hashtable { { "type", "F" } };

        var dt = _entityRepository.GetDataTable(MasterDataElement, filter, null, totalRecords, 1, ref totalRecords);
        foreach (DataRow row in dt.Rows)
        {
            yield return row["name"].ToString();
        }
    }

    public async IAsyncEnumerable<string> GetNameListAsync()
    {
        const int totalRecords = 10000;
        var filter = new Hashtable { { "type", "F" } };

        var dt = await _entityRepository.GetDataTableAsync(MasterDataElement, filter, null, totalRecords, 1, totalRecords);
        foreach (DataRow row in dt.Item1.Rows)
        {
            yield return row["name"].ToString();
        }
    }



    ///<inheritdoc cref="IDataDictionaryRepository.GetMetadata"/>
    public FormElement GetMetadata(string dictionaryName)
    {
        if (string.IsNullOrEmpty(dictionaryName))
            throw new ArgumentNullException(nameof(dictionaryName), Translate.Key("Dictionary invalid"));

        var filter = new Hashtable { { "name", dictionaryName } };
        var dataTable = _entityRepository.GetDataTable(MasterDataElement, filter);
        if (dataTable.Rows.Count == 0)
            throw new KeyNotFoundException(Translate.Key("Dictionary {0} not found", dictionaryName));

        var formElement = new FormElement();
        foreach (DataRow row in dataTable.Rows)
        {
            string json = row["json"].ToString();

            formElement = row["type"].ToString() switch
            {
                "F" => JsonConvert.DeserializeObject<FormElement>(json),
                _ => formElement
            };
        }
        

        return formElement;
    }



    ///<inheritdoc cref="IDataDictionaryRepository.InsertOrReplace"/>
    public void InsertOrReplace(FormElement metadata)
    {
        var values = GetFormElementHashtable(metadata);

        _entityRepository.SetValues(MasterDataElement, values);
    }

    public async Task InsertOrReplaceAsync(FormElement metadata)
    {
        var values = GetFormElementHashtable(metadata);

        await _entityRepository.SetValuesAsync(MasterDataElement, values);
    }

    private static Hashtable GetFormElementHashtable(FormElement metadata)
    {
        if (metadata == null)
            throw new ArgumentNullException(nameof(metadata));

        if (metadata == null)
            throw new ArgumentNullException(nameof(metadata));

        if (string.IsNullOrEmpty(metadata.Name))
            throw new ArgumentNullException(nameof(metadata.Name));

        string name = metadata.Name;

        DateTime dNow = DateTime.Now;

        string jsonForm = JsonConvert.SerializeObject(metadata);
        
        var values = new Hashtable
        {
            { "name", name },
            { "tablename", metadata.TableName },
            { "info", "" },
            { "type", "F" },
            { "owner", name },
            { "json", jsonForm },
            { "sync", metadata.Sync ? "1" : "0" },
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

        var dataTable = _entityRepository.GetDataTable(MasterDataElement, filters);
        if (dataTable.Rows.Count == 0)
            throw new KeyNotFoundException(Translate.Key("Dictionary {0} not found", dictionaryName));

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

        var filters = new Hashtable { { "name", dictionaryName } };

        var dataTable = await _entityRepository.GetDataTableAsync(MasterDataElement, filters);
        if (dataTable.Rows.Count == 0)
            throw new KeyNotFoundException(Translate.Key("Dictionary {0} not found", dictionaryName));

        foreach (DataRow row in dataTable.Rows)
        {
            var delFilter = new Hashtable
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
        int count = _entityRepository.GetCount(MasterDataElement, filter);
        return count > 0;
    }

    public async Task<bool> ExistsAsync(string dictionaryName)
    {
        if (string.IsNullOrEmpty(dictionaryName))
            throw new ArgumentNullException(nameof(dictionaryName));

        var filter = new Hashtable { { "name", dictionaryName } };
        int count = await _entityRepository.GetCountAsync(MasterDataElement, filter);
        return count > 0;
    }
    
    ///<inheritdoc cref="IDataDictionaryRepository.CreateStructureIfNotExists"/>
    public void CreateStructureIfNotExists()
    {
        if(!_entityRepository.TableExists(MasterDataElement.Name))
            _entityRepository.CreateDataModel(MasterDataElement);
    }
    
    public async Task CreateStructureIfNotExistsAsync()
    {
        if(!await _entityRepository.TableExistsAsync(MasterDataElement.Name))
            await _entityRepository.CreateDataModelAsync(MasterDataElement);
    }

    ///<inheritdoc cref="IDataDictionaryRepository.GetMetadataInfoList"/>
    public IEnumerable<FormElementInfo> GetMetadataInfoList(DataDictionaryFilter filter, string orderBy, int recordsPerPage, int currentPage, ref int totalRecords)
    {
        var filters = filter.ToHashtable();
        filters.Add("type","F");

        var dt = _entityRepository.GetDataTable(MasterDataElement, filters, orderBy, recordsPerPage, currentPage, ref totalRecords); 
        return dt.ToModelList<FormElementInfo>();
    }
    
    public async Task<IEnumerable<FormElementInfo>> GetMetadataInfoListAsync(DataDictionaryFilter filter, string orderBy, int recordsPerPage, int currentPage,
        int totalRecords)
    {
        var filters = filter.ToHashtable();
        filters.Add("type","F");

        var dt = await _entityRepository.GetDataTableAsync(MasterDataElement, filters, orderBy, recordsPerPage, currentPage,  totalRecords); 
        return dt.Item1.ToModelList<FormElementInfo>();
    }
}