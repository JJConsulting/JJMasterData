using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace JJMasterData.Commons.Data.Extensions;

public static class EnumerableExtensions
{
    public static DataTable ToDataTable<T>(this IEnumerable<T> items)
    {
        var dataTable = new DataTable(typeof(T).Name);
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in properties)
        {
            dataTable.Columns.Add(prop.Name);
        }
        foreach (var item in items)
        {
            var values = new object[properties.Length];
            for (var i = 0; i < properties.Length; i++)
            {
                values[i] = properties[i].GetValue(item, null);
            }
            dataTable.Rows.Add(values);
        }
        return dataTable;
    }
}