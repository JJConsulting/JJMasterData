using System;
using JJMasterData.Commons.Data.Entity.Models;

namespace JJMasterData.Core.DataDictionary.Structure;

public static class DataDictionaryStructure
{
    public const string Name = "name";
    public const string Type = "type";
    public const string TableName = "tablename";
    public const string Info = "info";
    public const string Owner = "owner";
    public const string EnableSynchronism = "sync";
    public const string LastModified = "modified";
    public const string LastModifiedFrom = "modified_from";
    public const string LastModifiedTo = "modified_to";
    public const string Json = "json";
    
    public static Element GetElement(string tableSchema, string tableName)
    {
        if (string.IsNullOrEmpty(tableName))
            throw new ArgumentNullException(nameof(tableName));
        
        var element = new Element(tableName, "Data Dictionary");
        element.Schema = tableSchema;
        element.Fields.AddPk(Type, "Type", FieldType.Varchar, 1, false, FilterMode.Equal);
        element.Fields[Type].EnableOnDelete = false;
        element.Fields.AddPk(Name, "Element", FieldType.NVarchar, 64, false, FilterMode.Equal);
        element.Fields.Add(TableName, "Table", FieldType.NVarchar, -1, false, FilterMode.MultValuesContain);
        element.Fields.Add(Json, "Filter for anything", FieldType.NVarchar, -1, false, FilterMode.Contain);
        element.Fields.Add(Info, "Info", FieldType.NVarchar, 150, false, FilterMode.None);
        element.Fields.Add(Owner, "Owner", FieldType.NVarchar, 64, false, FilterMode.None);
        element.Fields.Add(LastModified, "Last Modified", FieldType.DateTime, 15, true, FilterMode.None);
        element.Fields.Add(EnableSynchronism, "Enable Synchronism", FieldType.Bit, 1, false, FilterMode.Equal);
        
        return element;
    }
}