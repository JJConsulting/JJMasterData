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

    ///<inheritdoc cref="IDictionaryRepository.GetListDictionary(bool?)"/>
    public List<DataDictionary> GetListDictionary(bool? sync)
    {
        var list = new List<DataDictionary>();

        var filter = new Hashtable();
        if (sync.HasValue)
            filter.Add("sync", (bool)sync ? "1" : "0");

        string orderby = "name, type";
        string currentName = "";
        int tot = 1;
        var dt = _entityRepository.GetDataTable(GetStructure(), filter, orderby, 10000, 1, ref tot);
        DataDictionary currentParser = null;
        foreach (DataRow row in dt.Rows)
        {
            string name = row["name"].ToString();
            if (!currentName.Equals(name))
            {
                ApplyCompatibility(currentParser, name);

                currentName = name;
                list.Add(new DataDictionary());
                currentParser = list[list.Count - 1];
            }

            string json = row["json"].ToString();
            if (row["type"].ToString().Equals("T"))
            {
                currentParser.Table = JsonConvert.DeserializeObject<Element>(json);
            }
            else if (row["type"].ToString().Equals("F"))
            {
                currentParser.Form = JsonConvert.DeserializeObject<DataDictionaryForm>(json);
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

    ///<inheritdoc cref="IDictionaryRepository.GetListDictionaryName"/>
    public string[] GetListDictionaryName()
    {
        var list = new List<string>();
        int tot = 10000;
        var filter = new Hashtable();
        filter.Add("type", "F");

        var dt = _entityRepository.GetDataTable(GetStructure(), filter, null, tot, 1, ref tot);
        foreach (DataRow row in dt.Rows)
        {
            list.Add(row["name"].ToString());
        }

        return list.ToArray();
    }

    ///<inheritdoc cref="IDictionaryRepository.GetDictionary(string)"/>
    public DataDictionary GetDictionary(string elementName)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName), Translate.Key("Dictionary invalid"));

        Hashtable filter = new();
        filter.Add("name", elementName);
        DataTable dt = _entityRepository.GetDataTable(GetStructure(), filter);
        if (dt.Rows.Count == 0)
            throw new KeyNotFoundException(Translate.Key("Dictionary {0} not found", elementName));

        DataDictionary ret = new();
        foreach (DataRow row in dt.Rows)
        {
            string json = row["json"].ToString();
            if (row["type"].ToString().Equals("T"))
            {
                ret.Table = JsonConvert.DeserializeObject<Element>(json);
            }
            else if (row["type"].ToString().Equals("F"))
            {
                ret.Form = JsonConvert.DeserializeObject<DataDictionaryForm>(json);
            }
            else if (row["type"].ToString().Equals("L"))
            {
                ret.UIOptions = JsonConvert.DeserializeObject<UIOptions>(json);
            }
            else if (row["type"].ToString().Equals("A"))
            {
                ret.Api = JsonConvert.DeserializeObject<ApiSettings>(json);
            }
        }

        ApplyCompatibility(ret, elementName);

        return ret;
    }

    ///<inheritdoc cref="IDictionaryRepository.SetDictionary(DataDictionary)"/>
    public void SetDictionary(DataDictionary dictionary)
    {
        if (dictionary == null)
            throw new ArgumentNullException(nameof(dictionary));

        if (dictionary.Table == null)
            throw new ArgumentNullException(nameof(dictionary.Table));

        if (string.IsNullOrEmpty(dictionary.Table.Name))
            throw new ArgumentNullException(nameof(dictionary.Table.Name));

        var element = GetStructure();
        string name = dictionary.Table.Name;
        string jsonTable = JsonConvert.SerializeObject(dictionary.Table);

        DateTime dNow = DateTime.Now;

        var values = new Hashtable();
        values.Add("name", name);
        values.Add("tablename", dictionary.Table.TableName);
        values.Add("info", dictionary.Table.Info);
        values.Add("type", "T");
        values.Add("json", jsonTable);
        values.Add("sync", dictionary.Table.Sync ? "1" : "0");
        values.Add("modified", dNow);
        _entityRepository.SetValues(element, values);

        if (dictionary.Form != null)
        {
            string jsonForm = JsonConvert.SerializeObject(dictionary.Form);
            values.Clear();
            values.Add("name", name);
            values.Add("tablename", dictionary.Table.TableName);
            values.Add("info", "");
            values.Add("type", "F");
            values.Add("owner", name);
            values.Add("json", jsonForm);
            values.Add("sync", dictionary.Table.Sync ? "1" : "0");
            values.Add("modified", dNow);
            _entityRepository.SetValues(element, values);
        }

        if (dictionary.UIOptions != null)
        {
            string jsonForm = JsonConvert.SerializeObject(dictionary.UIOptions);
            values.Clear();
            values.Add("name", name);
            values.Add("tablename", dictionary.Table.TableName);
            values.Add("info", "");
            values.Add("type", "L");
            values.Add("owner", name);
            values.Add("json", jsonForm);
            values.Add("sync", dictionary.Table.Sync ? "1" : "0");
            values.Add("modified", dNow);
            _entityRepository.SetValues(element, values);
        }

        if (dictionary.Api != null)
        {
            string jsonForm = JsonConvert.SerializeObject(dictionary.Api);
            values.Clear();
            values.Add("name", name);
            values.Add("info", "");
            values.Add("type", "A");
            values.Add("owner", name);
            values.Add("json", jsonForm);
            values.Add("sync", dictionary.Table.Sync ? "1" : "0");
            values.Add("modified", dNow);
            _entityRepository.SetValues(element, values);
        }
    }

    ///<inheritdoc cref="IDictionaryRepository.DelDictionary(string)"/>
    public void DelDictionary(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException();

        var filters = new Hashtable();
        filters.Add("name", id);

        DataTable dt = _entityRepository.GetDataTable(GetStructure(), filters);
        if (dt.Rows.Count == 0)
            throw new KeyNotFoundException(Translate.Key("Dictionary {0} not found", id));

        var element = GetStructure();
        foreach (DataRow row in dt.Rows)
        {
            var delFilter = new Hashtable();
            delFilter.Add("name", id);
            delFilter.Add("type", row["type"].ToString());
            _entityRepository.Delete(element, delFilter);
        }
    }

    ///<inheritdoc cref="IDictionaryRepository.HasDictionary(string)"/>
    public bool HasDictionary(string elementName)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName));

        Hashtable filter = new Hashtable();
        filter.Add("name", elementName);
        int count = _entityRepository.GetCount(GetStructure(), filter);
        return count > 0;
    }

    ///<inheritdoc cref="IDictionaryRepository.ExecInitialSetup"/>
    public void ExecInitialSetup()
    {
        _entityRepository.CreateDataModel(GetStructure());
    }

    ///<inheritdoc cref="IDictionaryRepository.GetDataTable(Hashtable, string, int, int, ref int)"/>
    public DataTable GetDataTable(Hashtable filters, string orderby, int regperpage, int pag, ref int tot)
    {
        var element = GetStructure();
        return _entityRepository.GetDataTable(element,filters, orderby, regperpage, pag, ref tot);
    }

    internal static Element GetStructure()
    {
        var element = new Element(JJService.Settings.TableName, "Data Dictionaries");
        element.Fields.AddPK("type", "Type", FieldType.Varchar, 1, false, FilterMode.Equal);
        element.Fields["type"].EnableOnDelete = false;
        element.Fields.AddPK("name", "Dictionary Name", FieldType.NVarchar, 64, false, FilterMode.Equal);
        element.Fields.Add("namefilter", "Dictionary Name", FieldType.NVarchar, 30, false, FilterMode.Contain, FieldBehavior.ViewOnly);
        element.Fields.Add("tablename", "Table Name", FieldType.NVarchar, 64, false, FilterMode.MultValuesContain);
        element.Fields.Add("info", "Info", FieldType.NVarchar, 150, false, FilterMode.None);
        element.Fields.Add("owner", "Owner", FieldType.NVarchar, 64, false, FilterMode.None);
        element.Fields.Add("sync", "Sync", FieldType.Varchar, 1, false, FilterMode.Equal);
        element.Fields.Add("modified", "Last Modified", FieldType.DateTime, 15, true, FilterMode.Range);
        element.Fields.Add("json", "Object", FieldType.Text, 0, false, FilterMode.None);
        return element;
    }

    private void ApplyCompatibility(DataDictionary dicParser, string elementName)
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