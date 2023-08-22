using System;
using JJMasterData.Commons.Data.Entity;

namespace JJMasterData.Core.DataDictionary.Repository;

public class DataDictionaryStructure
{
    public const string Name = "name";
    public const string NameFilter = "namefilter";
    public const string Type = "type";
    public const string TableName = "tablename";
    public const string Info = "info";
    public const string Owner = "owner";
    public const string EnableWebApi = "sync";
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
        element.Fields.Add(EnableWebApi, "Sync", FieldType.Varchar, 1, false, FilterMode.Equal);
        element.Fields.Add(LastModified, "Last Modified", FieldType.DateTime, 15, true, FilterMode.Range);
        element.Fields.Add(Json, "Object", FieldType.Text, 0, false, FilterMode.Contain);
        
        return element;
    }
}