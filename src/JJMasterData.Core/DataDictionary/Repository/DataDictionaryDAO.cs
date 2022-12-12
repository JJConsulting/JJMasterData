using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Language;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace JJMasterData.Core.DataDictionary.Repository;

public class DataDictionaryDAO : IDataDictionaryRepository
{
    private readonly IEntityRepository _entityRepository;

    public DataDictionaryDAO(IEntityRepository entityRepository)
    {
        _entityRepository = entityRepository;
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
        var dt = _entityRepository.GetDataTable(DataDictionaryStructure.GetElement(), filter, orderBy, 10000, 1, ref tot);
        Metadata currentParser = null;
        foreach (DataRow row in dt.Rows)
        {
            string name = row["name"].ToString();
            if (!currentName.Equals(name))
            {
                DataDictionaryStructure.ApplyCompatibility(currentParser, name);

                currentName = name;
                list.Add(new Metadata());
                currentParser = list[list.Count - 1];
            }

            string json = row["json"].ToString();
            if (row["type"].ToString().Equals("T"))
            {
                currentParser!.Table = JsonConvert.DeserializeObject<Element>(json);
            }
            else if (row["type"].ToString().Equals("F"))
            {
                currentParser!.Form = JsonConvert.DeserializeObject<MetadataForm>(json);
            }
            else if (row["type"].ToString().Equals("L"))
            {
                currentParser!.UIOptions = JsonConvert.DeserializeObject<UIOptions>(json);
            }
            else if (row["type"].ToString().Equals("A"))
            {
                currentParser!.Api = JsonConvert.DeserializeObject<ApiSettings>(json);
            }
        }

        DataDictionaryStructure.ApplyCompatibility(currentParser, currentName);

        return list;
    }

    ///<inheritdoc cref="IDataDictionaryRepository.GetNameList"/>
    public IEnumerable<string> GetNameList()
    {
        int totalRecords = 10000;
        var filter = new Hashtable { { "type", "F" } };

        var dt = _entityRepository.GetDataTable(DataDictionaryStructure.GetElement(), filter, null, totalRecords, 1, ref totalRecords);
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
        var dataTable = _entityRepository.GetDataTable(DataDictionaryStructure.GetElement(), filter);
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
                    metadata.UIOptions = JsonConvert.DeserializeObject<UIOptions>(json);
                    break;
                case "A":
                    metadata.Api = JsonConvert.DeserializeObject<ApiSettings>(json);
                    break;
            }
        }

        DataDictionaryStructure.ApplyCompatibility(metadata, dictionaryName);

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

        var element = DataDictionaryStructure.GetElement();
        string name = metadata.Table.Name;
        string jsonTable = JsonConvert.SerializeObject(metadata.Table);

        DateTime dNow = DateTime.Now;

        var values = new Hashtable();
        values.Add("name", name);
        values.Add("tablename", metadata.Table.TableName);
        values.Add("info", metadata.Table.Info);
        values.Add("type", "T");
        values.Add("json", jsonTable);
        values.Add("sync", metadata.Table.Sync ? "1" : "0");
        values.Add("modified", dNow);
        _entityRepository.SetValues(element, values);

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
            _entityRepository.SetValues(element, values);
        }

        if (metadata.UIOptions != null)
        {
            string jsonForm = JsonConvert.SerializeObject(metadata.UIOptions);
            values.Clear();
            values.Add("name", name);
            values.Add("tablename", metadata.Table.TableName);
            values.Add("info", "");
            values.Add("type", "L");
            values.Add("owner", name);
            values.Add("json", jsonForm);
            values.Add("sync", metadata.Table.Sync ? "1" : "0");
            values.Add("modified", dNow);
            _entityRepository.SetValues(element, values);
        }

        if (metadata.Api != null)
        {
            string jsonForm = JsonConvert.SerializeObject(metadata.Api);
            values.Clear();
            values.Add("name", name);
            values.Add("info", "");
            values.Add("type", "A");
            values.Add("owner", name);
            values.Add("json", jsonForm);
            values.Add("sync", metadata.Table.Sync ? "1" : "0");
            values.Add("modified", dNow);
            _entityRepository.SetValues(element, values);
        }
    }

    ///<inheritdoc cref="IDataDictionaryRepository.Delete"/>
    public void Delete(string dictionaryName)
    {
        if (string.IsNullOrEmpty(dictionaryName))
            throw new ArgumentException();

        var filters = new Hashtable { { "name", dictionaryName } };

        var dataTable = _entityRepository.GetDataTable(DataDictionaryStructure.GetElement(), filters);
        if (dataTable.Rows.Count == 0)
            throw new KeyNotFoundException(Translate.Key("Dictionary {0} not found", dictionaryName));

        var element = DataDictionaryStructure.GetElement();
        foreach (DataRow row in dataTable.Rows)
        {
            var delFilter = new Hashtable();
            delFilter.Add("name", dictionaryName);
            delFilter.Add("type", row["type"].ToString());
            _entityRepository.Delete(element, delFilter);
        }
    }

    ///<inheritdoc cref="IDataDictionaryRepository.Exists"/>
    public bool Exists(string dictionaryName)
    {
        return _entityRepository.TableExists(dictionaryName);
    }

    ///<inheritdoc cref="IDataDictionaryRepository.CreateStructureIfNotExists"/>
    public void CreateStructureIfNotExists()
    {
        if(!Exists(JJService.Options.TableName))
            _entityRepository.CreateDataModel(DataDictionaryStructure.GetElement());
    }

    ///<inheritdoc cref="IDataDictionaryRepository.GetDataTable"/>
    public DataTable GetDataTable(DataDictionaryFilter filter, string orderBy, int recordsPerPage, int currentPage, ref int totalRecords)
    {
        var element = DataDictionaryStructure.GetElement();

        var filters = filter.ToHashtable();
        
        filters.Add("type","F");
        
        return _entityRepository.GetDataTable(element, filters, orderBy, recordsPerPage, currentPage, ref totalRecords);
    }
}