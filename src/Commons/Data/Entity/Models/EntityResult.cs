#nullable enable

using System.Diagnostics.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Data;

namespace JJMasterData.Commons.Data.Entity;

public class EntityResult
{
    internal List<IDictionary<string, object?>> Data { get; }
    
    /// <summary>
    /// TotalOfRecords can represent the total count of the data at a external source or the count of the data source.
    /// </summary>
    public int TotalOfRecords { get; }
    
    public EntityResult(DataTable dataTable, int totalOfRecords)
    {
        if (dataTable == null)
            throw new ArgumentNullException(nameof(dataTable));

        Data = ConvertDataTableToList(dataTable);
        TotalOfRecords = totalOfRecords;
    }
    
    public EntityResult(List<IDictionary<string, object?>> listData, int totalOfRecords)
    {
        Data = listData ?? throw new ArgumentNullException(nameof(listData));
        TotalOfRecords = totalOfRecords;
    }
    
    private static List<IDictionary<string, object?>> ConvertDataTableToList(DataTable dataTable)
    {
        var list = new List<IDictionary<string, object?>>();

        foreach (DataRow row in dataTable.Rows)
        {
            var dict = new Dictionary<string, object?>();

            foreach (DataColumn col in dataTable.Columns)
            {
                dict[col.ColumnName] = row.IsNull(col) ? null : row[col];
            }

            list.Add(dict);
        }

        return list;
    }
    
    public static List<IDictionary<string, object?>> ToList(EntityResult entityResult)
    {
        return entityResult.Data;
    }

    public List<IDictionary<string, object?>> ToList() => ToList(this);
    
    public DataTable ToDataTable() => ToDataTable(this);
    
    public static DataTable ToDataTable(EntityResult entityResult)
    {
        var dataTable = new DataTable();
        if (entityResult is { Data.Count: > 0 })
        {
            foreach (var key in entityResult.Data[0].Keys)
            {
                dataTable.Columns.Add(key, typeof(object));
            }

            foreach (var data in entityResult.Data)
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
    
    public static implicit operator EntityResult(DataTable dataTable)
    {
        return new EntityResult(dataTable, dataTable.Rows.Count);
    }
    
    public static implicit operator EntityResult(List<IDictionary<string, object?>> listData)
    {
        return new EntityResult(listData, listData.Count);
    }
    public static explicit operator List<IDictionary<string, object?>>(EntityResult entityResult) => ToList(entityResult);
    public static explicit operator DataTable(EntityResult entityResult) => ToDataTable(entityResult);
}

public class EntityResult<T>
{
    public required T Data { get; init; }
    public required int TotalOfRecords { get; init; }
    
    [SetsRequiredMembers]
    public EntityResult(T data, int totalOfRecords)
    {
        Data = data;
        TotalOfRecords = totalOfRecords;
    }

}