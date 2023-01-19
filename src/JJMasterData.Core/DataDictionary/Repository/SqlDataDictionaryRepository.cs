using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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
   
    ///<inheritdoc cref="IDataDictionaryRepository.GetMetadataList"/>
    public IEnumerable<Metadata> GetMetadataList(bool? sync = null)
    {
        var list = new List<Metadata>();

        var filter = new Hashtable();
        if (sync.HasValue)
            filter.Add("sync", (bool)sync ? "1" : "0");

        const string orderBy = "name, type";
        string currentName = "";
        int tot = 1;
        var dt = _entityRepository.GetDataTable(MasterDataElement, filter, orderBy, 10000, 1, ref tot);
        Metadata currentParser = null;
        foreach (DataRow row in dt.Rows)
        {
            string name = row["name"].ToString();
            if (!currentName.Equals(name))
            {
                DataDictionaryStructure.ApplyCompatibility(currentParser);

                currentName = name;
                list.Add(new Metadata());
                currentParser = list[list.Count - 1];
            }

            string json = row["json"].ToString();
            switch (row["type"].ToString()!)
            {
                case "T":
                    currentParser!.Table = JsonConvert.DeserializeObject<Element>(json);
                    break;
                case "F":
                    currentParser!.Form = JsonConvert.DeserializeObject<MetadataForm>(json);
                    break;
                case "L":
                    currentParser!.Options = JsonConvert.DeserializeObject<MetadataOptions>(json);
                    break;
                case "A":
                    currentParser!.ApiOptions = JsonConvert.DeserializeObject<MetadataApiOptions>(json);
                    break;
            }
        }

        DataDictionaryStructure.ApplyCompatibility(currentParser);

        return list;
    }

    ///<inheritdoc cref="IDataDictionaryRepository.GetNameList"/>
    public IEnumerable<string> GetNameList()
    {
        int totalRecords = 10000;
        var filter = new Hashtable { { "type", "F" } };

        var dt = _entityRepository.GetDataTable(MasterDataElement, filter, null, totalRecords, 1, ref totalRecords);
        foreach (DataRow row in dt.Rows)
        {
            yield return row["name"].ToString();
        }
    }

    ///<inheritdoc cref="IDataDictionaryRepository.GetMetadata"/>
    public Metadata GetMetadata(string dictionaryName)
    {
        if (string.IsNullOrEmpty(dictionaryName))
            throw new ArgumentNullException(nameof(dictionaryName), Translate.Key("Dictionary invalid"));

        var filter = new Hashtable { { "name", dictionaryName } };
        var dataTable = _entityRepository.GetDataTable(MasterDataElement, filter);
        if (dataTable.Rows.Count == 0)
            throw new KeyNotFoundException(Translate.Key("Dictionary {0} not found", dictionaryName));

        var metadata = new Metadata();
        foreach (DataRow row in dataTable.Rows)
        {
            string json = row["json"].ToString();
            
            switch (row["type"].ToString())
            {
                case "T":
                    metadata.Table = JsonConvert.DeserializeObject<Element>(json);
                    break;
                case "F":
                    metadata.Form = JsonConvert.DeserializeObject<MetadataForm>(json);
                    break;
                case "L":
                    metadata.Options = JsonConvert.DeserializeObject<MetadataOptions>(json);
                    break;
                case "A":
                    metadata.ApiOptions = JsonConvert.DeserializeObject<MetadataApiOptions>(json);
                    break;
            }
        }

        DataDictionaryStructure.ApplyCompatibility(metadata);

        return metadata;
    }

    ///<inheritdoc cref="IDataDictionaryRepository.InsertOrReplace"/>
    public void InsertOrReplace(Metadata metadata)
    {
        if (metadata == null)
            throw new ArgumentNullException(nameof(metadata));

        if (metadata.Table == null)
            throw new ArgumentNullException(nameof(metadata.Table));

        if (string.IsNullOrEmpty(metadata.Table.Name))
            throw new ArgumentNullException(nameof(metadata.Table.Name));
        
        string name = metadata.Table.Name;
        string jsonTable = JsonConvert.SerializeObject(metadata.Table);

        DateTime dNow = DateTime.Now;

        var values = new Hashtable
        {
            { "name", name },
            { "tablename", metadata.Table.TableName },
            { "info", metadata.Table.Info },
            { "type", "T" },
            { "json", jsonTable },
            { "sync", metadata.Table.Sync ? "1" : "0" },
            { "modified", dNow }
        };
        _entityRepository.SetValues(MasterDataElement, values);

        if (metadata.Form != null)
        {
            string jsonForm = JsonConvert.SerializeObject(metadata.Form);
            values.Clear();
            values.Add("name", name);
            values.Add("tablename", metadata.Table.TableName);
            values.Add("info", "");
            values.Add("type", "F");
            values.Add("owner", name);
            values.Add("json", jsonForm);
            values.Add("sync", metadata.Table.Sync ? "1" : "0");
            values.Add("modified", dNow);
            _entityRepository.SetValues(MasterDataElement, values);
        }

        if (metadata.Options != null)
        {
            string jsonForm = JsonConvert.SerializeObject(metadata.Options);
            values.Clear();
            values.Add("name", name);
            values.Add("tablename", metadata.Table.TableName);
            values.Add("info", "");
            values.Add("type", "L");
            values.Add("owner", name);
            values.Add("json", jsonForm);
            values.Add("sync", metadata.Table.Sync ? "1" : "0");
            values.Add("modified", dNow);
            _entityRepository.SetValues(MasterDataElement, values);
        }

        if (metadata.ApiOptions != null)
        {
            string jsonForm = JsonConvert.SerializeObject(metadata.ApiOptions);
            values.Clear();
            values.Add("name", name);
            values.Add("info", "");
            values.Add("type", "A");
            values.Add("owner", name);
            values.Add("json", jsonForm);
            values.Add("sync", metadata.Table.Sync ? "1" : "0");
            values.Add("modified", dNow);
            _entityRepository.SetValues(MasterDataElement, values);
        }
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
            var delFilter = new Hashtable();
            delFilter.Add("name", dictionaryName);
            delFilter.Add("type", row["type"].ToString());
            _entityRepository.Delete(MasterDataElement, delFilter);
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

    ///<inheritdoc cref="IDataDictionaryRepository.CreateStructureIfNotExists"/>
    public void CreateStructureIfNotExists()
    {
        if(!_entityRepository.TableExists(MasterDataElement.Name))
            _entityRepository.CreateDataModel(MasterDataElement);
    }

    ///<inheritdoc cref="IDataDictionaryRepository.GetMetadataInfoList"/>
    public IEnumerable<MetadataInfo> GetMetadataInfoList(DataDictionaryFilter filter, string orderBy, int recordsPerPage, int currentPage, ref int totalRecords)
    {
        var filters = filter.ToHashtable();
        filters.Add("type","F");

        var dt = _entityRepository.GetDataTable(MasterDataElement, filters, orderBy, recordsPerPage, currentPage, ref totalRecords); 
        return dt.ToModelList<MetadataInfo>();
    }
}