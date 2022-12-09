using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary.Action;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Repository;

public class DataDictionaryStructure
{
    public const string Name = "name";
    public const string NameFilter = "namefilter";
    public const string Type = "type";
    public const string TableName = "tablename";
    public const string Info = "info";
    public const string Owner = "owner";
    public const string Sync = "sync";
    public const string LastModified = "modified";
    public const string LastModifiedFrom = "modified_from";
    public const string LastModifiedTo = "modified_to";
    public const string Json = "json";
    
    public static Element GetElement()
    {
        var element = new Element(JJService.Options.TableName, "Data Dictionaries");
            
        element.Fields.AddPK(Type, "Type", FieldType.Varchar, 1, false, FilterMode.Equal);
        element.Fields[Type].EnableOnDelete = false;
        element.Fields.AddPK(Name, "Dictionary Name", FieldType.NVarchar, 64, false, FilterMode.Equal);
        element.Fields.Add(NameFilter, "Dictionary Name", FieldType.NVarchar, 30, false, FilterMode.Contain, FieldBehavior.ViewOnly);
        element.Fields.Add(TableName, "Table Name", FieldType.NVarchar, 64, false, FilterMode.MultValuesContain);
        element.Fields.Add(Info, "Info", FieldType.NVarchar, 150, false, FilterMode.None);
        element.Fields.Add(Owner, "Owner", FieldType.NVarchar, 64, false, FilterMode.None);
        element.Fields.Add(Sync, "Sync", FieldType.Varchar, 1, false, FilterMode.Equal);
        element.Fields.Add(LastModified, "Last Modified", FieldType.DateTime, 15, true, FilterMode.Range);
        element.Fields.Add(Json, "Object", FieldType.Text, 0, false, FilterMode.None);
        
        return element;
    }

    public static IList<IDictionary> GetStructure(Metadata metadata, DateTime modified)
    {
        string name = metadata.Table.Name;
        string jsonTable = JsonConvert.SerializeObject(metadata.Table);

        var structure = new List<IDictionary>();

        if (metadata.Table != null)
        {
            var values = new Hashtable
            {
                { Name, name },
                { TableName, metadata.Table.TableName },
                { Info, metadata.Table.Info },
                { Type, "T" },
                { Owner, null },
                { Json, jsonTable },
                { Sync, metadata.Table.Sync ? "1" : "0" },
                { LastModified, modified }
            };
            
            structure.Add(values);
        }
        
        if (metadata.Form != null)
        {
            string jsonForm = JsonConvert.SerializeObject(metadata.Form);

            var values = new Hashtable
            {
                { Name, name },
                { TableName, metadata.Table.TableName },
                { Info, metadata.Table.Info },
                { Type, "F" },
                { Owner, name },
                { Json, jsonForm },
                { Sync, metadata.Table.Sync ? "1" : "0" },
                { LastModified, modified }
            };

            structure.Add(values);
        }

        if (metadata.UIOptions != null)
        {
            string json = JsonConvert.SerializeObject(metadata.UIOptions);
            var values = new Hashtable
            {
                { Name, name },
                { TableName, metadata.Table.TableName },
                { Info, "" },
                { Type, "L" },
                { Owner, name },
                { Json, json },
                { Sync, metadata.Table.Sync ? "1" : "0" },
                { LastModified, modified }
            };
            structure.Add(values);
        }

        if (metadata.Api != null)
        {
            string jsonForm = JsonConvert.SerializeObject(metadata.Api);
            var values = new Hashtable
            {
                { Name, name },
                { TableName, metadata.Table.TableName },
                { Info, "" },
                { Type, "A" },
                { Owner, name },
                { Json, jsonForm },
                { Sync, metadata.Table.Sync ? "1" : "0" },
                { LastModified, modified }
            };
            structure.Add(values);
        }

        return structure;
    }

    public static void ApplyCompatibility(Metadata dicParser, string elementName)
    {
        if (dicParser == null)
            return;

        if (dicParser.Table == null)
            throw new Exception(Translate.Key("Dictionary {0} not found", elementName));

        //Nairobi
        dicParser.UIOptions ??= new UIOptions();

        dicParser.UIOptions.ToolBarActions ??= new GridToolBarActions();

        dicParser.UIOptions.GridActions ??= new GridActions();


        //Denver
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