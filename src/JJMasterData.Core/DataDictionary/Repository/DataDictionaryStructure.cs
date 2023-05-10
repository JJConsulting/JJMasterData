using System;
using System.Collections.Generic;
using System.Linq;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.DataDictionary.Action;

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
    
    public static Element GetElement(string tableName)
    {
        if (string.IsNullOrEmpty(tableName))
            throw new ArgumentNullException(nameof(tableName));
        
        var element = new Element(tableName, "Data Dictionaries");
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
    
    public static void ApplyCompatibility(Metadata dicParser)
    {
        if (dicParser?.Table == null)
            return;

        //Nairobi
        dicParser.Options ??= new MetadataOptions();

        dicParser.Options.ToolBarActions ??= new GridToolBarActions();

        dicParser.Options.GridActions ??= new GridActions();


        //Denver
        if (dicParser.ApiOptions == null)
        {
            dicParser.ApiOptions = new MetadataApiOptions();
            if (dicParser.Table.Sync)
            {
                dicParser.ApiOptions.EnableGetAll = true;
                dicParser.ApiOptions.EnableGetDetail = true;
                dicParser.ApiOptions.EnableAdd = true;
                dicParser.ApiOptions.EnableUpdate = true;
                dicParser.ApiOptions.EnableUpdatePart = true;
                dicParser.ApiOptions.EnableDel = true;
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
        foreach (var action in dicParser.Options.GridActions.GetAll()
                     .Where(action => action is UrlRedirectAction or InternalAction or ScriptAction or SqlCommandAction))
        {
            action.IsCustomAction = true;
        }

        foreach (var action in dicParser.Options.ToolBarActions
                     .GetAll()
                     .Where(action => action is UrlRedirectAction or InternalAction or ScriptAction or SqlCommandAction))
        {
            action.IsCustomAction = true;
        }

        //Alpha Centauri

        dicParser.Options.ToolBarActions.PythonActions ??= new List<PythonScriptAction>();

        dicParser.Options.GridActions.PythonActions ??= new List<PythonScriptAction>();

        //Sirius

        dicParser.Options.ToolBarActions.ExportAction.ProcessOptions ??= new ProcessOptions();

        dicParser.Options.ToolBarActions.ImportAction.ProcessOptions ??= new ProcessOptions();
    }
}