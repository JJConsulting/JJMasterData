using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.UI;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataManager.Services;

public class AuditLogService(IEntityRepository entityRepository, IOptionsSnapshot<MasterDataCoreOptions> options, IStringLocalizer<MasterDataResources> stringLocalizer)
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
    private IEntityRepository EntityRepository { get; } = entityRepository;
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;
    private MasterDataCoreOptions Options { get; } = options.Value;

    public async Task LogAsync(Element element,DataContext dataContext, Dictionary<string, object> formValues, CommandOperation action)
    {
        var values = new Dictionary<string, object>
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

        var logElement = GetElement(element.ConnectionId);
        await CreateTableIfNotExistsAsync(element.ConnectionId);
        await EntityRepository.InsertAsync(logElement, values);
    }

    private async Task CreateTableIfNotExistsAsync(Guid? connectionId)
    {
        if (!_hasAuditLogTable)
        {
            var logElement = GetElement(connectionId);
            if (!await EntityRepository.TableExistsAsync(logElement.TableName, logElement.ConnectionId))
                await EntityRepository.CreateDataModelAsync(logElement,[]);

            _hasAuditLogTable = true;
        }
    }

    private static string GetJsonFields(Dictionary<string, object>formValues)
    {
        var valuesAux = formValues
            .Where(item => item.Value is not DBNull)
            .ToDictionary(item => item.Key, item => item.Value);

        return JsonConvert.SerializeObject(valuesAux);
    }

    public static string GetKey(Element element, Dictionary<string, object>values)
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

    public Element GetElement(Guid? elementConnectionString)
    {
        string tableName = Options.AuditLogTableName;
        var element = new Element(tableName, StringLocalizer["Audit Log"]);
        element.Fields.AddPk(DicId, "Id", FieldType.Int, 1, true, FilterMode.Equal);
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

    public FormElement GetFormElement(FormElement formElement)
    {
        var auditLogFormElement = new FormElement(GetElement(formElement.ConnectionId));
        auditLogFormElement.Fields[DicId].VisibleExpression = "val:0";
        auditLogFormElement.Fields[DicName].VisibleExpression = "val:0";
        auditLogFormElement.Fields[DicBrowser].VisibleExpression = "val:0";
        auditLogFormElement.Fields[DicJson].VisibleExpression = "val:0";
        auditLogFormElement.Fields[DicModified].Component = FormComponent.DateTime;

        auditLogFormElement.Options.GridTableActions.Clear();
        auditLogFormElement.Options.GridToolbarActions.InsertAction.SetVisible(false);
        
        var origin = auditLogFormElement.Fields[DicOrigin];
        origin.Component = FormComponent.ComboBox;
        origin.DataItem = new FormElementDataItem
        {
            GridBehavior = DataItemGridBehavior.Icon,
            Items = []
        };
        foreach (int i in Enum.GetValues(typeof(DataContextSource)))
        {
            var item = new DataItemValue(i.ToString(), Enum.GetName(typeof(DataContextSource), i));
            origin.DataItem.Items.Add(item);
        }

        var action = auditLogFormElement.Fields[DicAction];
        action.Component = FormComponent.ComboBox;
        action.DataItem = new FormElementDataItem
        {
            GridBehavior = DataItemGridBehavior.Icon,
            Items = [],
            ShowIcon = true
        };
        action.DataItem.Items.Add(new DataItemValue(((int)CommandOperation.Insert).ToString(), "Added", IconType.Plus, GetInsertColor()));
        action.DataItem.Items.Add(new DataItemValue(((int)CommandOperation.Update).ToString(), "Edited", IconType.Pencil, GetUpdateColor()));
        action.DataItem.Items.Add(new DataItemValue(((int)CommandOperation.Delete).ToString(), "Deleted", IconType.Trash, GetDeleteColor()));
        var btnViewLog = new ScriptAction
        {
            Icon = IconType.Eye,
            Tooltip = "View"
        };
        btnViewLog.Name = nameof(btnViewLog);
        btnViewLog.OnClientClick = $"AuditLogViewHelper.viewAuditLog('{formElement.Name}','{{{DicId}}}');";

        auditLogFormElement.Options.GridTableActions.Add(btnViewLog);
        return auditLogFormElement;
    }
    
    internal static string GetUpdateColor()
    {
        return BootstrapHelper.Version == 3 ? "#ffbf00" : "var(--bs-warning)";
    }

    internal static string GetInsertColor()
    {
        return BootstrapHelper.Version == 3 ? "#387c44" : "var(--bs-success)";
    }

    internal static string GetDeleteColor()
    {
        return BootstrapHelper.Version == 3 ? "#b20000" : "var(--bs-danger)";
    }


}