using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary.Action;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace JJMasterData.Core.DataDictionary.Repository;

public class DictionaryDao : IDictionaryRepository
{
    private readonly IEntityRepository _entityRepository;

    public DictionaryDao(IEntityRepository entityRepository)
    {
        _entityRepository = entityRepository;
    }

    
    public IList<Metadata> GetMetadataList(bool? sync)
    {
        var list = new List<Metadata>();

        var filter = new Hashtable();
        if (sync.HasValue)
            filter.Add("sync", (bool)sync ? "1" : "0");

        string orderBy = "name, type";
        string currentName = "";
        int tot = 1;
        var dt = _entityRepository.GetDataTable(DataDictionaryStructure.GetElement(), filter, orderBy, 10000, 1, ref tot);
        Metadata currentParser = null;
        foreach (DataRow row in dt.Rows)
        {
            string name = row["name"].ToString();
            if (!currentName.Equals(name))
            {
                ApplyCompatibility(currentParser, name);

                currentName = name;
                list.Add(new Metadata());
                currentParser = list[list.Count - 1];
            }

            string json = row["json"].ToString();
            if (row["type"].ToString().Equals("T"))
            {
                currentParser.Table = JsonConvert.DeserializeObject<Element>(json);
            }
            else if (row["type"].ToString().Equals("F"))
            {
                currentParser.Form = JsonConvert.DeserializeObject<MetadataForm>(json);
            }
            else if (row["type"].ToString().Equals("L"))
            {
                currentParser.UIOptions = JsonConvert.DeserializeObject<UIOptions>(json);
            }
            else if (row["type"].ToString().Equals("A"))
            {
                currentParser.Api = JsonConvert.DeserializeObject<ApiSettings>(json);
            }
        }

        ApplyCompatibility(currentParser, currentName);

        return list;
    }

    
    public IEnumerable<string> GetNameList()
    {
        var list = new List<string>();
        int tot = 10000;
        var filter = new Hashtable();
        filter.Add("type", "F");

        var dt = _entityRepository.GetDataTable(DataDictionaryStructure.GetElement(), filter, null, tot, 1, ref tot);
        foreach (DataRow row in dt.Rows)
        {
            list.Add(row["name"].ToString());
        }

        return list.ToArray();
    }

    
    public Metadata GetMetadata(string dictionaryName)
    {
        if (string.IsNullOrEmpty(dictionaryName))
            throw new ArgumentNullException(nameof(dictionaryName), Translate.Key("Dictionary invalid"));

        var filter = new Hashtable();
        filter.Add("name", dictionaryName);
        DataTable dt = _entityRepository.GetDataTable(DataDictionaryStructure.GetElement(), filter);
        if (dt.Rows.Count == 0)
            throw new KeyNotFoundException(Translate.Key("Dictionary {0} not found", dictionaryName));

        var metadata = new Metadata();
        foreach (DataRow row in dt.Rows)
        {
            string json = row["json"].ToString();
            if (row["type"].ToString().Equals("T"))
            {
                metadata.Table = JsonConvert.DeserializeObject<Element>(json);
            }
            else if (row["type"].ToString().Equals("F"))
            {
                metadata.Form = JsonConvert.DeserializeObject<MetadataForm>(json);
            }
            else if (row["type"].ToString().Equals("L"))
            {
                metadata.UIOptions = JsonConvert.DeserializeObject<UIOptions>(json);
            }
            else if (row["type"].ToString().Equals("A"))
            {
                metadata.Api = JsonConvert.DeserializeObject<ApiSettings>(json);
            }
        }

        ApplyCompatibility(metadata, dictionaryName);

        return metadata;
    }

    
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

    
    public void Delete(string dictionaryName)
    {
        if (string.IsNullOrEmpty(dictionaryName))
            throw new ArgumentException();

        var filters = new Hashtable();
        filters.Add("name", dictionaryName);

        DataTable dt = _entityRepository.GetDataTable(DataDictionaryStructure.GetElement(), filters);
        if (dt.Rows.Count == 0)
            throw new KeyNotFoundException(Translate.Key("Dictionary {0} not found", dictionaryName));

        var element = DataDictionaryStructure.GetElement();
        foreach (DataRow row in dt.Rows)
        {
            var delFilter = new Hashtable();
            delFilter.Add("name", dictionaryName);
            delFilter.Add("type", row["type"].ToString());
            _entityRepository.Delete(element, delFilter);
        }
    }

    
    public bool Exists(string elementName)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName));

        Hashtable filter = new Hashtable();
        filter.Add("name", elementName);
        int count = _entityRepository.GetCount(DataDictionaryStructure.GetElement(), filter);
        return count > 0;
    }

    
    public void CreateStructureIfNotExists()
    {
        if(!Exists(JJService.Options.TableName))
            _entityRepository.CreateDataModel(DataDictionaryStructure.GetElement());
    }

    
    public DataTable GetDataTable(DataDictionaryFilter filter, string orderBy, int recordsPerPage, int currentPage, ref int totalRecords)
    {
        var element = DataDictionaryStructure.GetElement();

        var filters = filter.ToHashtable();
        
        filters.Add("type","F");
        
        return _entityRepository.GetDataTable(element, filters, orderBy, recordsPerPage, currentPage, ref totalRecords);
    }

    private void ApplyCompatibility(Metadata dicParser, string elementName)
    {
        if (dicParser == null)
            return;

        if (dicParser.Table == null)
            throw new Exception(Translate.Key("Dictionary {0} not found", elementName));

        //Mantendo compatibilidate versão Nairobi
        dicParser.UIOptions ??= new UIOptions();

        dicParser.UIOptions.ToolBarActions ??= new GridToolBarActions();

        dicParser.UIOptions.GridActions ??= new GridActions();
        //Fim compatibilidate Nairobi


        //Mantendo compatibilidate versão Denver 27/10/2020 (remover após 1 ano)
        if (dicParser.Api == null)
        {
            dicParser.Api = new ApiSettings();
            if (dicParser.Table.Sync)
            {
                dicParser.Api.EnableGetAll = true;
                dicParser.Api.EnableGetDetail = true;
                dicParser.Api.EnableAdd = true;
                dicParser.Api.EnableUpdate = true;
                dicParser.Api.EnableUpdatePart = true;
                dicParser.Api.EnableDel = true;
            }
        }

        if (string.IsNullOrEmpty(dicParser.Table.TableName))
        {
            dicParser.Table.TableName = dicParser.Table.Name;
        }
        //Fim compatibilidate Denver

        //Tokio
        if (dicParser.Form is { Panels: null }) dicParser.Form.Panels = new List<FormElementPanel>();

        //Professor
        if (dicParser.Form != null)
        {
            foreach (var field in dicParser.Form.FormFields)
            {
                if (field.DataItem is not { DataItemType: DataItemType.Manual })
                    continue;

                if (field.DataItem.Command != null && !string.IsNullOrEmpty(field.DataItem.Command.Sql))
                    field.DataItem.DataItemType = DataItemType.SqlCommand;
                else if (field.DataItem.ElementMap != null && !string.IsNullOrEmpty(field.DataItem.ElementMap.ElementName))
                    field.DataItem.DataItemType = DataItemType.Dictionary;
            }
        }

        //Arturito
        foreach (var action in dicParser.UIOptions.GridActions.GetAll()
                     .Where(action => action is UrlRedirectAction or InternalAction or ScriptAction or SqlCommandAction))
        {
            action.IsCustomAction = true;
        }

        foreach (var action in dicParser.UIOptions.ToolBarActions
                     .GetAll()
                     .Where(action => action is UrlRedirectAction or InternalAction or ScriptAction or SqlCommandAction))
        {
            action.IsCustomAction = true;
        }

        //Alpha Centauri

        dicParser.UIOptions.ToolBarActions.PythonActions ??= new List<PythonScriptAction>();

        dicParser.UIOptions.GridActions.PythonActions ??= new List<PythonScriptAction>();

        //Sirius

        dicParser.UIOptions.ToolBarActions.ExportAction.ProcessOptions ??= new ProcessOptions();

        dicParser.UIOptions.ToolBarActions.ImportAction.ProcessOptions ??= new ProcessOptions();
    }

}