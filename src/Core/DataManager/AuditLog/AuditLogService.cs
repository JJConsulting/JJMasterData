using System;
using System.Collections;
using System.Linq;
using System.Text;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Options;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Options;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataManager.AuditLog;

public class AuditLogService : IAuditLogService
{
    public const string DicId = "id";
    public const string DicName = "dictionary";
    public const string DicKey = "recordKey";
    public const string DicAction = "actionType";
    public const string DicOrigin = "origin";
    public const string DicModified = "modified";
    public const string DicUserid = "userId";
    public const string DicIp = "ip";
    public const string DicBrowser = "browser";
    public const string DicJson = "json";

    private static bool _hasAuditLogTable;
    public IEntityRepository EntityRepository { get; private set; }
    public JJMasterDataCommonsOptions CommonsOptions { get; }
    public JJMasterDataCoreOptions CoreOptions { get; }

    public AuditLogService(IEntityRepository entityRepository, IOptions<JJMasterDataCommonsOptions> commonsOptions, IOptions<JJMasterDataCoreOptions> coreOptions)
    {
        EntityRepository = entityRepository;
        CommonsOptions = commonsOptions.Value;
        CoreOptions = coreOptions.Value;
    }

    public void AddLog(Element element,DataContext dataContext, IDictionary formValues, CommandOperation action)
    {
        var values = new Hashtable
        {
            { DicName, element.Name },
            { DicKey, GetKey(element, formValues) },
            { DicAction, (int)action },
            { DicOrigin, (int)dataContext.Source },
            { DicModified, DateTime.Now },
            { DicUserid, dataContext.UserId },
            { DicIp, dataContext.IpAddress },
            { DicBrowser, dataContext.BrowserInfo },
            { DicJson, GetJsonFields(formValues) }
        };

        var logElement = GetElement();
        CreateTableIfNotExist();
        EntityRepository.Insert(logElement, values);
    }

    public void CreateTableIfNotExist()
    {
        if (!_hasAuditLogTable)
        {
            var logElement = GetElement();
            if (!EntityRepository.TableExists(logElement.TableName))
                EntityRepository.CreateDataModel(logElement);

            _hasAuditLogTable = true;
        }
    }

    private string GetJsonFields(IDictionary formValues)
    {
        var valuesAux = new Hashtable();
        foreach (DictionaryEntry item in formValues)
        {
            if (item.Value != DBNull.Value)
                valuesAux.Add(item.Key, item.Value);
        }

        return JsonConvert.SerializeObject(valuesAux);
    }

    public string GetKey(Element element, IDictionary values)
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
        string tableName = CoreOptions.AuditLogTableName;
        var element = new Element(tableName, "Log")
        {
            CustomProcNameGet = CommonsOptions.GetWriteProcedureName(tableName),
            CustomProcNameSet = CommonsOptions.GetWriteProcedureName(tableName)
        };
        element.Fields.AddPK(DicId, "Id", FieldType.Int, 1, true, FilterMode.Equal);
        element.Fields.Add(DicName, "Dictionary Name", FieldType.NVarchar, 64, true, FilterMode.Equal);
        element.Fields.Add(DicAction, "Action", FieldType.Int, 1, true, FilterMode.Equal);
        element.Fields.Add(DicModified, "Date", FieldType.DateTime, 15, true, FilterMode.Range);
        element.Fields.Add(DicUserid, "User Id", FieldType.Varchar, 30, false, FilterMode.Contain);
        element.Fields.Add(DicIp, "IP Address", FieldType.Varchar, 45, false, FilterMode.Contain);
        element.Fields.Add(DicBrowser, "Browser", FieldType.Varchar, 100, false, FilterMode.None);
        element.Fields.Add(DicOrigin, "Origin", FieldType.Int, 1, true, FilterMode.Equal);
        element.Fields.Add(DicKey, "Record Key", FieldType.Varchar, 100, true, FilterMode.Equal);
        element.Fields.Add(DicJson, "Object", FieldType.Text, 0, false, FilterMode.None);

        return element;
    }

    public FormElement GetFormElement()
    {
        var form = new FormElement(GetElement());
        form.Fields[DicId].VisibleExpression = "val:0";
        form.Fields[DicName].VisibleExpression = "val:0";
        form.Fields[DicBrowser].VisibleExpression = "val:0";
        form.Fields[DicJson].VisibleExpression = "val:0";
        form.Fields[DicModified].Component = FormComponent.DateTime;

        var origin = form.Fields[DicOrigin];
        origin.Component = FormComponent.ComboBox;
        origin.DataItem.ReplaceTextOnGrid = true;
        foreach (int i in Enum.GetValues(typeof(DataContextSource)))
        {
            var item = new DataItemValue(i.ToString(), Enum.GetName(typeof(DataContextSource), i));
            origin.DataItem.Items.Add(item);
        }

        var action = form.Fields[DicAction];
        action.Component = FormComponent.ComboBox;
        action.DataItem.ReplaceTextOnGrid = true;
        action.DataItem.ShowImageLegend = true;
        action.DataItem.Items.Add(new DataItemValue(((int)CommandOperation.Insert).ToString(), "Added", IconType.Plus, "#387c44"));
        action.DataItem.Items.Add(new DataItemValue(((int)CommandOperation.Update).ToString(), "Edited", IconType.Pencil, "#ffbf00"));
        action.DataItem.Items.Add(new DataItemValue(((int)CommandOperation.Delete).ToString(), "Deleted", IconType.Trash, "#b20000"));

        return form;
    }

}