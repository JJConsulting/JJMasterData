using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Logging;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Action;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.DictionaryDAL;

public class DictionaryDao
{
    private IDataAccess _dataAccess;
    private Factory _factory;
    private DicTranslate _dicTranslate;

    public Factory Factory
    {
        get
        {
            _factory = new Factory(_dataAccess);
            if (_factory == null)
            {
                if (_dataAccess != null)
                    _factory = new Factory(_dataAccess);
                else
                    _factory = new Factory();
            }

            return _factory;
        }
    }

    private DicTranslate DicTranslate
    {
        get
        {
            if (_dicTranslate == null)
            {
                _dicTranslate = new DicTranslate();
            }

            return _dicTranslate;
        }
    }


    public DictionaryDao()
    {
        _dataAccess = JJService.DataAccess;
    }

    public DictionaryDao(IDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

       

    /// <summary>
    /// Recupera uma lista de metadados armazenados no banco de dados
    /// </summary>
    /// <param name="sync">
    /// true=Somente itens que serão sincronizados. 
    /// false=Somente itens sem sincronismo
    /// null=Todos
    /// </param>
    /// <remarks>
    /// Metodo normalmente utilizado para sincronismo do dicionários entre sistemas.
    /// Permitindo remondar a herança original no sistema legado.
    /// </remarks>
    public List<DicParser> GetListDictionary(bool? sync)
    {
        var list = new List<DicParser>();

        var filter = new Hashtable();
        if (sync.HasValue)
            filter.Add("sync", (bool)sync ? "1" : "0");

        string orderby = "name, type";
        string currentName = "";
        int tot = 1;
        var dt = Factory.GetDataTable(Factory.GetStructure(), filter, orderby, 10000, 1, ref tot);
        DicParser currentParser = null;
        foreach (DataRow row in dt.Rows)
        {
            string name = row["name"].ToString();
            if (!currentName.Equals(name))
            {
                ApplyCompatibility(currentParser, name);

                currentName = name;
                list.Add(new DicParser());
                currentParser = list[list.Count - 1];
            }

            string json = row["json"].ToString();
            if (row["type"].ToString().Equals("T"))
            {
                currentParser.Table = JsonConvert.DeserializeObject<Element>(json);
            }
            else if (row["type"].ToString().Equals("F"))
            {
                currentParser.Form = JsonConvert.DeserializeObject<DicFormParser>(json);
            }
            else if (row["type"].ToString().Equals("L"))
            {
                currentParser.UIOptions = JsonConvert.DeserializeObject<UIOptions>(json);
            }
            else if (row["type"].ToString().Equals("A"))
            {
                currentParser.Api = JsonConvert.DeserializeObject<DicApiSettings>(json);
            }
        }

        ApplyCompatibility(currentParser, currentName);

        return list;
    }


    public string GetListDictionaryJson(bool? sync = null)
    {
        string orderby = "name, type";
        var filter = new Hashtable();
        if (sync.HasValue)
            filter.Add("sync", (bool)sync ? "1" : "0");

        int tot = 1;
        var dt = Factory.GetDataTable(Factory.GetStructure(), filter, orderby, 10000, 1, ref tot);

        if (dt.Rows.Count == 0)
            return null;

        var sJson = new StringBuilder();
        sJson.AppendLine("[");

        bool isFirst = true;
        string currentName = null;
        string jsTable = null;
        string jsForm = null;
        string jsUI = null;
        string jsApi = null;
        foreach (DataRow row in dt.Rows)
        {
            string name = row["name"].ToString();

            if (currentName == null)
                currentName = name;

            if (!currentName.Equals(name))
            {
                if (isFirst)
                    isFirst = false;
                else
                    sJson.AppendLine(",");

                sJson.Append(GetElementJson(jsTable, jsForm, jsUI, jsApi));

                currentName = name;
                jsTable = null;
                jsForm = null;
                jsUI = null;
                jsApi = null;
            }

            if (row["type"].ToString().Equals("T"))
                jsTable = row["json"].ToString();
            else if (row["type"].ToString().Equals("F"))
                jsForm = row["json"].ToString();
            else if (row["type"].ToString().Equals("L"))
                jsUI = row["json"].ToString();
            else if (row["type"].ToString().Equals("A"))
                jsApi = row["json"].ToString();
        }

        if (sJson.Length > 1)
            sJson.AppendLine(",");

        sJson.AppendLine(GetElementJson(jsTable, jsForm, jsUI, jsApi));
        sJson.AppendLine("]");

        return sJson.ToString();
    }

    private string GetElementJson(string jsTable, string jsForm, string jsUI, string jsApi)
    {
        var sJson = new StringBuilder();
        DicApiSettings apiSettings = null;
        if (jsApi != null)
        {
            apiSettings = JsonConvert.DeserializeObject<DicApiSettings>(jsApi);
        }

        sJson.AppendLine("{");
        sJson.Append("\"table\": ");
        sJson.Append(jsTable);
        if (apiSettings != null && apiSettings.HasSetMehtod())
        {
            if (jsForm != null)
            {
                sJson.Append(",");
                sJson.Append("\"form\": ");
                sJson.Append(jsForm);
            }

            if (jsUI != null)
            {
                sJson.Append(",");
                sJson.Append("\"uioptions\": ");
                sJson.Append(jsUI);
            }
        }
        sJson.AppendLine("}");

        return sJson.ToString();
    }

    public string[] GetListDictionaryName()
    {
        var list = new List<string>();
        int tot = 10000;
        var filter = new Hashtable();
        filter.Add("type", "F");

        var dt = Factory.GetDataTable(Factory.GetStructure(), filter, null, tot, 1, ref tot);
        foreach (DataRow row in dt.Rows)
        {
            list.Add(row["name"].ToString());
        }

        return list.ToArray();
    }

    /// <summary>
    /// Retorna metadados armazenados no banco de dados
    /// </summary>
    /// <param name="elementName"></param>
    /// <returns>
    /// Retorna Objeto armazenado no banco. 
    /// Responsável por montar o Element, FormElement 
    /// e outras configurações de layout
    /// </returns>
    public DicParser GetDictionary(string elementName)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName), Translate.Key("Dictionary invalid"));

        Hashtable filter = new();
        filter.Add("name", elementName);
        DataTable dt = Factory.GetDataTable(Factory.GetStructure(), filter);
        if (dt.Rows.Count == 0)
            throw new KeyNotFoundException(Translate.Key("Dictionary {0} not found", elementName));

        DicParser ret = new();
        foreach (DataRow row in dt.Rows)
        {
            string json = row["json"].ToString();
            if (row["type"].ToString().Equals("T"))
            {
                ret.Table = JsonConvert.DeserializeObject<Element>(json);
            }
            else if (row["type"].ToString().Equals("F"))
            {
                ret.Form = JsonConvert.DeserializeObject<DicFormParser>(json);
            }
            else if (row["type"].ToString().Equals("L"))
            {
                ret.UIOptions = JsonConvert.DeserializeObject<UIOptions>(json);
            }
            else if (row["type"].ToString().Equals("A"))
            {
                ret.Api = JsonConvert.DeserializeObject<DicApiSettings>(json);
            }
        }

        ApplyCompatibility(ret, elementName);

        return ret;
    }

    private void ApplyCompatibility(DicParser dicParser, string elementName)
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
            dicParser.Api = new DicApiSettings();
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


    /// <summary>
    /// Persiste o dicionário no banco de dados
    /// </summary>
    public void SetDictionary(DicParser dictionary)
    {
        if (dictionary == null)
            throw new ArgumentNullException(nameof(dictionary));

        if (dictionary.Table == null)
            throw new ArgumentNullException(nameof(dictionary.Table));

        if (string.IsNullOrEmpty(dictionary.Table.Name))
            throw new ArgumentNullException(nameof(dictionary.Table.Name));

        var cmds = new List<DataAccessCommand>();
        var element = Factory.GetStructure();
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
        var cmdTable = Factory.Provider.GetWriteCommand("", element, values);
        cmds.Add(cmdTable);

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
            var cmdForm = Factory.Provider.GetWriteCommand("", element, values);
            cmds.Add(cmdForm);
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
            var cmdForm = Factory.Provider.GetWriteCommand("", element, values);
            cmds.Add(cmdForm);
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
            var cmdForm = Factory.Provider.GetWriteCommand("", element, values);
            cmds.Add(cmdForm);
        }

        _dataAccess.SetCommand(cmds);
    }

    /// <summary>
    /// Exclui o elemento no banco de dados
    /// </summary>
    /// <param name="id">Nome do dicionário</param>
    public void DelDictionary(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException();

        var filters = new Hashtable();
        filters.Add("name", id);

        DataTable dt = Factory.GetDataTable(Factory.GetStructure(), filters);
        if (dt.Rows.Count == 0)
            throw new KeyNotFoundException(Translate.Key("Dictionary {0} not found", id));

        var listcmd = new List<DataAccessCommand>();
        foreach (DataRow row in dt.Rows)
        {
            var delFilter = new Hashtable();
            delFilter.Add("name", id);
            delFilter.Add("type", row["type"].ToString());
            var cmd = Factory.Provider.GetDeleteScript(Factory.GetStructure(), delFilter);
            listcmd.Add(cmd);
        }
        _dataAccess.SetCommand(listcmd);
    }

    /// <summary>
    /// Verifica se o dicionário existe
    /// </summary>
    public bool HasDictionary(string elementName)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName));

        Hashtable filter = new Hashtable();
        filter.Add("name", elementName);
        int count = Factory.GetCount(Factory.GetStructure(), filter);
        return count > 0;
    }

    /// <summary>
    /// Cria Estrutura do dicionário de dados
    /// </summary>
    public void CreateStructure()
    {
        Factory.CreateDataModel(Factory.GetStructure());
    }

    /// <summary>
    /// Analisa uma lista de elementos retornando quantos registros precisam ser sincronizados
    /// </summary>
    /// <param name="userId">Id do Usuários</param>
    /// <param name="listSync">Lista de elementos</param>
    /// <param name="showLogInfo">Grava log detalhado de cada operação</param>
    /// <param name="maxRecordsAllowed">
    /// Numero máximo de registros permitidos, 
    /// se ultrapassar esse numero uma exeção será disparada
    /// </param>
    public DicSyncInfo GetSyncInfo(string userId, DicSyncParam[] listSync, bool showLogInfo, long maxRecordsAllowed = 0)
    {
        if (listSync == null)
            throw new ArgumentNullException(nameof(DicSyncParam));

        if (listSync.Length == 0)
            throw new ArgumentException(Translate.Key("DicSyncParam invalid"));

        var dStart = DateTime.Now;
        var dictionaries = GetListDictionary(true);
        var syncInfo = new DicSyncInfo();
        syncInfo.ServerDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        int totRecords = 0;
        foreach (var os in listSync)
        {
            var dStartObj = DateTime.Now;
            var dictionary = dictionaries.Find(x => x.Table.Name.Equals(os.Name));
            if (dictionary == null)
                throw new Exception(Translate.Key("Dictionary {0} not found or not configured for sync", os.Name));

            var filters = GetSyncInfoFilter(userId, dictionary, os.Filters);
            var info = new DicSyncInfoElement();
            info.Name = os.Name;
            info.RecordSize = Factory.GetCount(dictionary.Table, filters);
            totRecords += info.RecordSize;

            TimeSpan tsObj = DateTime.Now - dStartObj;
            info.ProcessMilliseconds = tsObj.TotalMilliseconds;
            syncInfo.ListElement.Add(info);

            if (showLogInfo)
            {
                Log.AddInfo($"- {os.Name}: [{info.RecordSize}] {tsObj.TotalMilliseconds}ms\r\n");
            }

            if (maxRecordsAllowed > 0 && info.RecordSize > maxRecordsAllowed)
            {
                throw new Exception(Translate.Key("Number maximum of records exceeded on {0}, contact the administrator.", os.Name));
            }
        }

        TimeSpan ts = DateTime.Now - dStart;
        syncInfo.TotalProcessMilliseconds = ts.TotalMilliseconds;

        var sLog = new StringBuilder();
        sLog.AppendLine($"UserId: {userId}");
        sLog.Append(Translate.Key("Synchronizing"));
        sLog.Append(listSync.Length);
        sLog.Append(Translate.Key("objects"));
        sLog.Append(" ");
        sLog.AppendLine(" ...");
        sLog.AppendLine(Translate.Key("{0} records analyzed in {1}", totRecords, Format.FormatTimeSpan(ts)));
        Log.AddInfo(sLog.ToString());

        if (syncInfo.ListElement.Count == 0)
            throw new KeyNotFoundException(Translate.Key("No dictionary found"));

        return syncInfo;
    }


    private Hashtable GetSyncInfoFilter(string userId, DicParser dictionary, Hashtable osFilters)
    {
        var filters = new Hashtable();
        var fields = dictionary.Table.Fields;
        if (osFilters != null)
        {
            foreach (DictionaryEntry osFilter in osFilters)
            {
                if (!fields.ContainsKey(osFilter.Key.ToString()))
                    continue;

                filters.Add(fields[osFilter.Key.ToString()].Name, osFilter.Value);
            }
        }

        string fieldApplyUser = dictionary.Api.ApplyUserIdOn;
        if (!string.IsNullOrEmpty(fieldApplyUser))
        {
            if (!filters.ContainsKey(fieldApplyUser))
            {
                filters.Add(fieldApplyUser, userId);
            }
            else
            {
                if (!filters[fieldApplyUser].ToString().Equals(userId))
                    throw new UnauthorizedAccessException(Translate.Key("Access denied to change user filter on {0}", dictionary.Table.Name));
            }
        }

        return filters;
    }

    /// <summary>
    /// Recupera uma lista de objetos de masterdata em json
    /// </summary>
    /// <returns>Retorna json</returns>
    public string GetListFormElementJson()
    {
        return GetListFormElementJson(null);
    }

    /// <summary>
    /// Recupera uma lista de objetos de masterdata em json
    /// </summary>
    /// <param name="sync">
    /// true=Somente itens que serão sincronizados. 
    /// false=Somente itens sem sincronismo
    /// null=Todos
    /// </param>
    /// <returns>Retorna json</returns>
    public string GetListFormElementJson(bool? sync)
    {
        StringBuilder sJson = new StringBuilder();
        sJson.Append("[");

        var listElement = new List<Element>();
        var filter = new Hashtable();
        filter.Add("type", "T");
        if (sync.HasValue)
            filter.Add("sync", (bool)sync ? "1" : "0");

        int tot = 1000;
        var dt = Factory.GetDataTable(Factory.GetStructure(), filter, null, 1000, 1, ref tot);

        bool isFirst = true;
        foreach (DataRow row in dt.Rows)
        {
            if (isFirst)
                isFirst = false;
            else
                sJson.Append(",");

            sJson.Append("{");
            sJson.Append("\"table\":");
            sJson.Append(row["json"]);

            Hashtable filterForm = new Hashtable();
            filterForm.Add("name", row["name"].ToString());
            filterForm.Add("type", "F");
            Hashtable resultElement = Factory.GetFields(Factory.GetStructure(), filterForm);
            if (resultElement != null)
            {
                sJson.Append(",\"form\":");
                sJson.Append(resultElement["json"]);
            }
            sJson.Append("}");
        }
        sJson.Append("]");

        return sJson.ToString();
    }
        
    /// <summary>
    /// Recupera um objeto com dados do formulário
    /// </summary>
    /// <param name="elementName">Nome do dicionário</param>
    /// <returns>
    /// Retorna um FormElement com os principais dados do formulário.
    /// Herda de Element
    /// </returns>
    public FormElement GetFormElement(string elementName)
    {
        var dao = new DictionaryDao();
        var dic = dao.GetDictionary(elementName);

        if (dic == null || dic.Form == null)
            throw new ArgumentException(Translate.Key("Dictionary {0} not found", elementName), "elementName");

        return dic.GetFormElement();
    }

    /// <summary>
    /// Persiste as configurações do formulário no banco de dados
    /// </summary>
    /// <param name="formElement">Elemento base com a estrutura do formulário.</param>
    public void SetFormElement(FormElement formElement)
    {
        if (formElement == null)
            throw new ArgumentNullException(nameof(formElement));

        if (string.IsNullOrEmpty(formElement.Name))
            throw new ArgumentException(Translate.Key("Invalid dictionary name"));


        for (int i = 0; i < formElement.Fields.Count; i++)
        {
            formElement.Fields[i].Order = i + 1;
        }

        var dic = new DicParser
        {
            Table = formElement.DeepCopy(),
            Form = new DicFormParser(formElement)
        };

        SetDictionary(dic);
    }


    /// <summary>
    /// Constroi uma nova instancia do objeto Element a partir da estrutura de uma tabela
    /// </summary>
    /// <param name="tableName">Nome da tabela</param>
    public Element GetElementFromTable(string tableName)
    {
        if (string.IsNullOrEmpty(tableName))
            throw new ArgumentNullException(nameof(tableName));

        if (!_dataAccess.TableExists(tableName))
            throw new Exception(Translate.Key("Table {0} not found", tableName));

        Element element = null;
        try
        {
            if (DataAccess.MSSQL.Equals(_dataAccess.ConnectionProvider))
                element = ((MSSQLProvider)Factory.Provider).GetElementFromTable(tableName, ref _dataAccess);
        }
        catch (Exception)
        {
            // ignored
        }

        if (element == null)
        {
            string sql = "select * from " + tableName + " where 1=2";
            var dt = _dataAccess.GetDataTable(sql);
            element = new FormElement(dt);
            element.Name = tableName;
        }

        return element;
    }

}