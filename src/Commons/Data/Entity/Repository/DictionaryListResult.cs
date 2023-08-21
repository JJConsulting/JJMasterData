#nullable enable
using System.Collections.Generic;
using System.Data;

namespace JJMasterData.Commons.Data.Entity.Repository;

public class DictionaryListResult : ListResult<Dictionary<string, object?>>
{
    internal DictionaryListResult(List<Dictionary<string, object?>> list, int totalOfRecords) : base(list,totalOfRecords)
    {
    }

    public static DictionaryListResult FromDataTable(DataTable dataTable)
    {
        var list = new List<Dictionary<string, object?>>();

        foreach (DataRow row in dataTable.Rows)
        {
            var dict = new Dictionary<string, object?>();

            foreach (DataColumn col in dataTable.Columns)
            {
                dict[col.ColumnName] = row.IsNull(col) ? null : row[col];
            }

            list.Add(dict);
        }

        return new DictionaryListResult(list,list.Count);
    }
    
    
    public DataTable ToDataTable()
    {
        var dataTable = new DataTable();
        
        if (Data.Count <= 0) 
            return dataTable;
        
        foreach (var key in Data[0].Keys)
        {
            var valueType = Data[0][key]?.GetType() ?? typeof(object);
            dataTable.Columns.Add(key, valueType);
        }

        foreach (var data in Data)
        {
            var row = dataTable.NewRow();
            foreach (var kvp in data)
            {
                row[kvp.Key] = kvp.Value;
            }
            dataTable.Rows.Add(row);
        }
        return dataTable;
    }
}