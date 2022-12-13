using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace JJMasterData.Core.DataDictionary.Repository;

/// <summary>
/// The Data Dictionaries (metadata) are stored in the Database
/// </summary>
public class DataDictionaryFilter
{
    public string Name { get; set; }
    public IList<string> ContainsTableName { get; set; }
    public DateTime? LastModifiedFrom { get; set; }
    public DateTime? LastModifiedTo { get; set; }

    public static DataDictionaryFilter GetInstance(Hashtable filter)
    {
        var result = new DataDictionaryFilter();

        if (filter.ContainsKey(DataDictionaryStructure.Name))
        {
            result.Name = filter[DataDictionaryStructure.Name].ToString();
        }

        if (filter.ContainsKey(DataDictionaryStructure.TableName))
        {
            result.ContainsTableName = filter[DataDictionaryStructure.TableName].ToString().Split(',');
        }
        
        if (filter.ContainsKey(DataDictionaryStructure.LastModifiedFrom))
        {
            result.LastModifiedFrom = DateTime.Parse(filter[DataDictionaryStructure.LastModifiedFrom].ToString());
        }
        
        if (filter.ContainsKey(DataDictionaryStructure.LastModifiedTo))
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