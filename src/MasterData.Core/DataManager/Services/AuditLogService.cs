#nullable disable warnings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Serialization;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Models;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.DataManager.Services;

public class AuditLogService(
    IEntityRepository entityRepository,
    IOptionsSnapshot<MasterDataCoreOptions> options)
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

    public async Task LogAsync(Element element,DataContext dataContext, Dictionary<string, object?> formValues, CommandOperation action)
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

        var logElement = GetElement();
        await entityRepository.InsertAsync(logElement, values);
    }

    public async Task CreateStructureIfNotExistsAsync()
    {
        var logElement = GetElement();
        if (!await entityRepository.TableExistsAsync(logElement.Schema, logElement.TableName))
            await entityRepository.CreateDataModelAsync(logElement);
    }

    private static string GetJsonFields(Dictionary<string, object>formValues)
    {
        var valuesAux = formValues
            .Where(item => item.Value is not DBNull)
            .ToDictionary(item => item.Key, item => item.Value);

        return JsonSerializer.Serialize(valuesAux, MasterDataJsonSerializerOptions.Default);
    }

    public static string GetKey(Element element, Dictionary<string, object>values)
    {
        var key = new StringBuilder();
        var pks = element.Fields.FindAll(x => x.IsPk);
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
        string tableName = options.Value.AuditLogTableName;
        var element = new Element(tableName, "Audit Log");
        element.Schema = options.Value.AuditLogTableSchema;
        element.Fields.AddPk(DicId, "Id", FieldType.Int, 1, true, FilterMode.Equal);
        element.Fields.Add(DicName, "Dictionary Name", FieldType.NVarchar, 64, true, FilterMode.Equal);
        element.Fields.Add(DicAction, "Action", FieldType.Int, 1, true, FilterMode.Equal);
        element.Fields.Add(DicModified, "Date", FieldType.DateTime, 15, true, FilterMode.Range);
        element.Fields.Add(DicUserid, "User Id", FieldType.Varchar, 30, false, FilterMode.Contain);
        element.Fields.Add(DicIp, "IP Address", FieldType.Varchar, 45, false, FilterMode.Contain);
        element.Fields.Add(DicBrowser, "Browser", FieldType.Varchar, 100, false, FilterMode.None);
        element.Fields.Add(DicOrigin, "Origin", FieldType.Int, 1, true, FilterMode.Equal);
        element.Fields.Add(DicKey, "Record Key", FieldType.Varchar, 100, true, FilterMode.Contain);
        element.Fields.Add(DicJson, "Object", FieldType.Text, 0, false, FilterMode.None);

        element.UseReadProcedure = false;
        element.UseWriteProcedure = false;
        
        return element;
    }

}
