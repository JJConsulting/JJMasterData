#nullable enable
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace JJMasterData.Commons.Data.Entity;

public class DataSource : EntityResult<Dictionary<string, dynamic?>>
{
    internal DataSource(List<Dictionary<string, dynamic?>> list, int totalOfRecords) : base(list,totalOfRecords)
    {
    }
    
    internal DataSource(DataTable dataTable, int totalOfRecords) : base(ConvertDataTableToList(dataTable),totalOfRecords)
    {
    }
    
    private static List<Dictionary<string, dynamic?>> ConvertDataTableToList(DataTable dataTable)
    {
        var list = new List<Dictionary<string, dynamic?>>();

        foreach (DataRow row in dataTable.Rows)
        {
            var dict = new Dictionary<string, dynamic?>();

            foreach (DataColumn col in dataTable.Columns)
            {
                dict[col.ColumnName] = row.IsNull(col) ? null : row[col];
            }

            list.Add(dict);
        }

        return list;
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