using System;
using System.Collections;
using System.Linq;
using System.Text;
using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Options;
using JJMasterData.Core.DataDictionary;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataManager.AuditLog;

public class AuditLogService
{
    public const string DIC_ID = "id";
    public const string DIC_NAME = "dictionary";
    public const string DIC_KEY = "recordKey";
    public const string DIC_ACTION = "actionType";
    public const string DIC_ORIGIN = "origin";
    public const string DIC_MODIFIED = "modified";
    public const string DIC_USERID = "userId";
    public const string DIC_IP = "ip";
    public const string DIC_BROWSER = "browser";
    public const string DIC_JSON = "json";

    private static bool _hasAuditLogTable;
    private IDataAccess _dataAccess;
    private Factory _factory;

    public DataContext Data { get; set; }


    /// <summary>
    /// Objeto responsável por fazer toda a comunicação com o banco de dados
    /// </summary>
    public IDataAccess DataAccess
    {
        get => _dataAccess ??= JJService.DataAccess;
        set => _dataAccess = value;
    }

    /// <summary>
    /// Objeto responsável por traduzir o elemento base em comandos para o banco de dados
    /// </summary>
    public Factory Factory
    {
        get => _factory ??= new Factory(DataAccess);
        set => _factory = value;
    }

    public AuditLogService(DataContext data)
    {
        Data = data;
    }

    public void AddLog(Element element, Hashtable formValues, CommandType action)
    {
        var values = new Hashtable
        {
            { DIC_NAME, element.Name },
            { DIC_KEY, GetKey(element, formValues) },
            { DIC_ACTION, (int)action },
            { DIC_ORIGIN, (int)Data.Source },
            { DIC_MODIFIED, DateTime.Now },
            { DIC_USERID, Data.UserId },
            { DIC_IP, Data.IpAddress },
            { DIC_BROWSER, Data.BrowserInfo },
            { DIC_JSON, GetJsonFields(formValues) }
        };

        var logElement = GetElement();
        CreateTableIfNotExist();
        Factory.Insert(logElement, values);
    }

    public void CreateTableIfNotExist()
    {
        if (!_hasAuditLogTable)
        {
            var logElement = GetElement();
            if (!DataAccess.TableExists(logElement.TableName))
                Factory.CreateDataModel(logElement);

            _hasAuditLogTable = true;
        }
    }

    private string GetJsonFields(Hashtable formValues)
    {
        var valuesAux = new Hashtable();
        foreach (DictionaryEntry item in formValues)
        {
            if (item.Value != DBNull.Value)
                valuesAux.Add(item.Key, item.Value);
        }

        return JsonConvert.SerializeObject(valuesAux);
    }

    public string GetKey(Element element, Hashtable values)
    {
        var key = new StringBuilder();
        var pks = element.Fields.ToList().FindAll(x => x.IsPk);
        foreach (var field in pks)
        {
            if (key.Length > 0)
                key.Append(";");

            key.Append(values[field.Name]);
        }

        return key.ToString();
    }

    public Element GetElement()
    {
        string tableName = ConfigurationHelper.GetMasterDataAuditLog();
        var element = new Element(tableName, "Log")
        {
            CustomProcNameGet = JJMasterDataOptions.GetDefaultProcNameGet(tableName),
            CustomProcNameSet = JJMasterDataOptions.GetDefaultProcNameSet(tableName)
        };
        element.Fields.AddPK(DIC_ID, "Id", FieldType.Int, 1, true, FilterMode.Equal);
        element.Fields.Add(DIC_NAME, "Dictionary Name", FieldType.NVarchar, 64, true, FilterMode.Equal);
        element.Fields.Add(DIC_ACTION, "Action", FieldType.Int, 1, true, FilterMode.Equal);
        element.Fields.Add(DIC_MODIFIED, "Date", FieldType.DateTime, 15, true, FilterMode.Range);
        element.Fields.Add(DIC_USERID, "User Id", FieldType.Varchar, 30, false, FilterMode.Contain);
        element.Fields.Add(DIC_IP, "IP Address", FieldType.Varchar, 45, false, FilterMode.Contain);
        element.Fields.Add(DIC_BROWSER, "Browser", FieldType.Varchar, 100, false, FilterMode.None);
        element.Fields.Add(DIC_ORIGIN, "Origin", FieldType.Int, 1, true, FilterMode.Equal);
        element.Fields.Add(DIC_KEY, "Record Key", FieldType.Varchar, 100, true, FilterMode.Equal);
        element.Fields.Add(DIC_JSON, "Object", FieldType.Text, 0, false, FilterMode.None);

        return element;
    }

    public FormElement GetFormElement()
    {
        var form = new FormElement(GetElement());
        form.Fields[DIC_ID].VisibleExpression = "val:0";
        form.Fields[DIC_NAME].VisibleExpression = "val:0";
        form.Fields[DIC_BROWSER].VisibleExpression = "val:0";
        form.Fields[DIC_JSON].VisibleExpression = "val:0";
        form.Fields[DIC_MODIFIED].Component = FormComponent.DateTime;

        var origin = form.Fields[DIC_ORIGIN];
        origin.Component = FormComponent.ComboBox;
        origin.DataItem.ReplaceTextOnGrid = true;
        foreach (int i in Enum.GetValues(typeof(DataContextSource)))
        {
            var item = new DataItemValue(i.ToString(), Enum.GetName(typeof(DataContextSource), i));
            origin.DataItem.Items.Add(item);
        }

        var action = form.Fields[DIC_ACTION];
        action.Component = FormComponent.ComboBox;
        action.DataItem.ReplaceTextOnGrid = true;
        action.DataItem.ShowImageLegend = true;
        action.DataItem.Items.Add(new DataItemValue(((int)CommandType.Insert).ToString(), "Added", IconType.Plus, "#387c44"));
        action.DataItem.Items.Add(new DataItemValue(((int)CommandType.Update).ToString(), "Edited", IconType.Pencil, "#ffbf00"));
        action.DataItem.Items.Add(new DataItemValue(((int)CommandType.Delete).ToString(), "Deleted", IconType.Trash, "#b20000"));

        return form;
    }

}