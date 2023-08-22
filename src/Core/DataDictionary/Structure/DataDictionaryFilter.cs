#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace JJMasterData.Core.DataDictionary.Repository;

public class DataDictionaryFilter
{
    public string? Name { get;private set; }
    public IList<string>? ContainsTableName { get;private set; }
    public DateTime? LastModifiedFrom { get;private set; }
    public DateTime? LastModifiedTo { get;private  set; }
    public string? Json { get; private set; }
    public static DataDictionaryFilter FromDictionary(IDictionary<string, object>filter)
    {
        var result = new DataDictionaryFilter();

        if (filter.TryGetValue(DataDictionaryStructure.Name, out var name))
        {
            result.Name = name.ToString();
        }

        if (filter.TryGetValue(DataDictionaryStructure.TableName, out var tableName))
        {
            result.ContainsTableName = tableName.ToString().Split(',');
        }
        
        if (filter.TryGetValue(DataDictionaryStructure.LastModifiedFrom, out var lastModifiedFrom))
        {
            result.LastModifiedFrom = DateTime.Parse(lastModifiedFrom.ToString());
        }
        
        if (filter.TryGetValue(DataDictionaryStructure.LastModifiedTo, out var lastModifiedTo))
        {
            result.LastModifiedTo = DateTime.Parse(lastModifiedTo.ToString());
        }
        
        if (filter.TryGetValue(DataDictionaryStructure.Json, out var json))
        {
            result.Json = json?.ToString();
        }

        return result;
    }
    public IDictionary<string, object> ToDictionary()
    {
        var result = new Dictionary<string, object>();
        if (Name != null)
        {
            result[DataDictionaryStructure.NameFilter] = Name;
        }
        
        if (ContainsTableName != null && ContainsTableName.Any())
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