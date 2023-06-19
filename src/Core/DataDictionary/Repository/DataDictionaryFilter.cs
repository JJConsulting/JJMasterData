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

    public static DataDictionaryFilter GetInstance(IDictionary filter)
    {
        var result = new DataDictionaryFilter();

        if (filter.Contains(DataDictionaryStructure.Name))
        {
            result.Name = filter[DataDictionaryStructure.Name].ToString();
        }

        if (filter.Contains(DataDictionaryStructure.TableName))
        {
            result.ContainsTableName = filter[DataDictionaryStructure.TableName].ToString().Split(',');
        }
        
        if (filter.Contains(DataDictionaryStructure.LastModifiedFrom))
        {
            result.LastModifiedFrom = DateTime.Parse(filter[DataDictionaryStructure.LastModifiedFrom].ToString());
        }
        
        if (filter.Contains(DataDictionaryStructure.LastModifiedTo))
        {
            result.LastModifiedTo = DateTime.Parse(filter[DataDictionaryStructure.LastModifiedTo].ToString());
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