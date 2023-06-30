using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace JJMasterData.Core.DataDictionary.Repository;

public class DataDictionaryFilter
{
    public string Name { get; set; }
    public IList<string> ContainsTableName { get; set; }
    public DateTime? LastModifiedFrom { get; set; }
    public DateTime? LastModifiedTo { get; set; }

    public static DataDictionaryFilter GetInstance(IDictionary<string,dynamic>filter)
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

        return result;
    }
    public Hashtable ToHashtable()
    {
        var result = new Hashtable();
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

        return result;
    }

}