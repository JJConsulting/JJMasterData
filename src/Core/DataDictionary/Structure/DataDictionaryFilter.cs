#nullable enable
using System;
using System.Collections.Generic;

namespace JJMasterData.Core.DataDictionary.Structure;

public class DataDictionaryFilter
{
    public string? Name { get; set; }
    public IList<string>? ContainsTableName { get; set; }
    public DateTime? LastModifiedFrom { get; set; }
    public DateTime? LastModifiedTo { get; set; }
    public string? Json { get; set; }
    
    public DataDictionaryFilter()
    {
    }
    
    public static DataDictionaryFilter FromDictionary(Dictionary<string, object?>filter)
    {
        var result = new DataDictionaryFilter();

        if (filter.TryGetValue(DataDictionaryStructure.Name, out var name) && name != null)
        {
            result.Name = name.ToString();
        }

        if (filter.TryGetValue(DataDictionaryStructure.TableName, out var tableName) && tableName != null)
        {
            result.ContainsTableName = tableName.ToString()!.Split(',');
        }
        
        if (filter.TryGetValue(DataDictionaryStructure.LastModifiedFrom, out var lastModifiedFrom) && lastModifiedFrom != null)
        {
            result.LastModifiedFrom = DateTime.Parse(lastModifiedFrom.ToString()!);
        }
        
        if (filter.TryGetValue(DataDictionaryStructure.LastModifiedTo, out var lastModifiedTo) && lastModifiedTo != null)
        {
            result.LastModifiedTo = DateTime.Parse(lastModifiedTo.ToString()!);
        }
        
        
        if (filter.TryGetValue(DataDictionaryStructure.Json, out var json))
        {
            result.Json = json?.ToString();
        }

        return result;
    }
    public Dictionary<string, object> ToDictionary()
    {
        var result = new Dictionary<string, object>();
        
        
        if (Name != null)
        {
            result[DataDictionaryStructure.Name] = Name;
        }
        
        if (ContainsTableName?.Count > 0)
        {
            string tableNameFilter = string.Empty;
            for (int i = 0; i < ContainsTableName.Count; i++)
            {
                if (i > 0)
                    tableNameFilter += ",";

                tableNameFilter += ContainsTableName[i];
            }
            result[DataDictionaryStructure.TableName] = tableNameFilter;
        }
        
        if (LastModifiedFrom != null)
        {
            result[DataDictionaryStructure.LastModifiedFrom] = LastModifiedFrom;
        }
        
        if (LastModifiedTo != null)
        {
            result[DataDictionaryStructure.LastModifiedTo] = LastModifiedTo;
        }
        
        if (Json != null)
        {
            result[DataDictionaryStructure.Json] = Json;
        }
 
        
        return result;
    }

}