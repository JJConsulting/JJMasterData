﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Options;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataManager.Services;

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
    private IEntityRepository EntityRepository { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    private JJMasterDataCoreOptions Options { get; }

    public AuditLogService(IEntityRepository entityRepository, IOptions<JJMasterDataCoreOptions> options, IStringLocalizer<JJMasterDataResources> stringLocalizer)
    {
        EntityRepository = entityRepository;
        StringLocalizer = stringLocalizer;
        Options = options.Value;
    }

    public async Task LogAsync(Element element,DataContext dataContext, IDictionary<string,dynamic> formValues, CommandOperation action)
    {
        var values = new Dictionary<string,dynamic>()
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
        await CreateTableIfNotExistsAsync();
        await EntityRepository.InsertAsync(logElement, values);
    }

    public async Task CreateTableIfNotExistsAsync()
    {
        if (!_hasAuditLogTable)
        {
            var logElement = GetElement();
            if (!await EntityRepository.TableExistsAsync(logElement.TableName))
                await EntityRepository.CreateDataModelAsync(logElement);

            _hasAuditLogTable = true;
        }
    }

    private static string GetJsonFields(IDictionary<string,dynamic>formValues)
    {
        var valuesAux = formValues
            .Where(item => item.Value is not DBNull)
            .ToDictionary(item => item.Key, item => item.Value);

        return JsonConvert.SerializeObject(valuesAux);
    }

    public string GetKey(Element element, IDictionary<string,dynamic>values)
    {
        var key = new StringBuilder();
        var pks = element.Fields.ToList().FindAll(x => x.IsPk);
        foreach (var field in pks)
        {
            if (key.Length > 0)
                key.Append(';');

            key.Append(values[field.Name]);
        }

        return key.ToString();
    }

    public Element GetElement()
    {
        string tableName = Options.AuditLogTableName;
        var element = new Element(tableName, StringLocalizer["Audit Log"])
        {
            CustomProcNameGet = Options.GetReadProcedureName(tableName),
            CustomProcNameSet = Options.GetWriteProcedureName(tableName)
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

        form.Options.GridTableActions.Clear();
        
        var origin = form.Fields[DicOrigin];
        origin.Component = FormComponent.ComboBox;
        origin.DataItem!.ReplaceTextOnGrid = true;
        foreach (int i in Enum.GetValues(typeof(DataContextSource)))
        {
            var item = new DataItemValue(i.ToString(), Enum.GetName(typeof(DataContextSource), i));
            origin.DataItem.Items.Add(item);
        }

        var action = form.Fields[DicAction];
        action.Component = FormComponent.ComboBox;
        action.DataItem!.ReplaceTextOnGrid = true;
        action.DataItem.ShowImageLegend = true;
        action.DataItem.Items.Add(new DataItemValue(((int)CommandOperation.Insert).ToString(), "Added", IconType.Plus, "#387c44"));
        action.DataItem.Items.Add(new DataItemValue(((int)CommandOperation.Update).ToString(), "Edited", IconType.Pencil, "#ffbf00"));
        action.DataItem.Items.Add(new DataItemValue(((int)CommandOperation.Delete).ToString(), "Deleted", IconType.Trash, "#b20000"));

        return form;
    }

}