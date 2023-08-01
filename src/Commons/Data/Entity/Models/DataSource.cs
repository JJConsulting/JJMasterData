#nullable enable
namespace JJMasterData.Commons.Data.Entity;
using System;
using System.Collections.Generic;
using System.Data;

public class DataSource
{
    public List<IDictionary<string, dynamic?>> Data { get; }

    public int TotalOfRecords => Data.Count;
    
    public DataSource(DataTable dataTable)
    {
        if (dataTable == null)
            throw new ArgumentNullException(nameof(dataTable));

        Data = ConvertDataTableToList(dataTable);
    }
    
    public DataSource(List<IDictionary<string, dynamic?>> listData)
    {
        Data = listData ?? throw new ArgumentNullException(nameof(listData));
    }
    
    private static List<IDictionary<string, dynamic?>> ConvertDataTableToList(DataTable dataTable)
    {
        var list = new List<IDictionary<string, dynamic?>>();

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
    
    public static List<IDictionary<string, dynamic?>> ToList(DataSource dataSource)
    {
        return dataSource.Data;
    }

    public List<IDictionary<string,dynamic?>> ToList() => ToList(this);
    
    public DataTable ToDataTable() => ToDataTable(this);
    
    public static DataTable ToDataTable(DataSource dataSource)
    {
        var dataTable = new DataTable();
        if (dataSource is { Data.Count: > 0 })
        {
            foreach (var key in dataSource.Data[0].Keys)
            {
                dataTable.Columns.Add(key, typeof(object));
            }

            foreach (var data in dataSource.Data)
            {
                var row = dataTable.NewRow();
                foreach (var kvp in data)
                {
                    row[kvp.Key] = kvp.Value;
                }
                dataTable.Rows.Add(row);
            }
        }
        return dataTable;
    }
    
    public static implicit operator DataSource(DataTable dataTable)
    {
        return new DataSource(dataTable);
    }
    
    public static implicit operator DataSource(List<IDictionary<string, dynamic?>> listData)
    {
        return new DataSource(listData);
    }
    public static explicit operator List<IDictionary<string,dynamic?>>(DataSource dataSource) => ToList(dataSource);
    public static explicit operator DataTable(DataSource dataSource) => ToDataTable(dataSource);
}
