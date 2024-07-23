#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Extensions;

namespace JJMasterData.Commons.Util;

public static class EnumerableHelper
{
    public static List<Dictionary<string,object?>>[] ConvertDataSetToArray(DataSet dataSet)
    {
        var result = new List<List<Dictionary<string,object?>>>();

        foreach (DataTable table in dataSet.Tables)
        {
            result.Add(ConvertToDictionaryList(table));
        }

        return result.ToArray();
    }
    

    public static List<Dictionary<string, object?>> ConvertToDictionaryList(DataTable dataTable)
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

        return list;
    }
    
    public static DataTable ConvertToDataTable(Element element, IList<Dictionary<string, object?>> data)
    {
        if (data == null || data.Count == 0)
            throw new ArgumentException("Data cannot be null or empty.");

        var table = new DataTable();
        
        foreach (var column in data[0].Keys)
        {
            table.Columns.Add(column, GetTypeFromField(element.Fields[column].DataType));
        }
        foreach (var item in data)
        {
            var row = table.NewRow();

            foreach (var key in item.Keys)
            {
                var value = item[key];
                row[key] = value ?? DBNull.Value;
            }
            table.Rows.Add(row);
        }

        return table;
    }
    
    public static DataTable ConvertToDataTable<T>(IEnumerable<T> list)
    {
        var table = CreateDataTable<T>();
        var entityType = typeof(T);
        var properties = TypeDescriptor.GetProperties(entityType);
        foreach (var item in list)
        {
            var row = table.NewRow();
            foreach (PropertyDescriptor prop in properties)
            {
                if (item is null)
                    row[prop.Name] = DBNull.Value;
                else
                    row[prop.Name] = prop.GetValue(item);
            }
            table.Rows.Add(row);
        }
        return table;
    }
    
    private static Type GetTypeFromField(FieldType fieldType)
    {
        switch (fieldType)
        {
            case FieldType.Date:
            case FieldType.DateTime:
            case FieldType.DateTime2:
            case FieldType.Time:
                return typeof(DateTime);
            case FieldType.Float:
                return typeof(double);
            case FieldType.Int:
                return typeof(int);
            case FieldType.NText:
            case FieldType.NVarchar:
            case FieldType.Text:
            case FieldType.Varchar:
                return typeof(string);
            case FieldType.Bit:
                return typeof(bool);
            case FieldType.UniqueIdentifier:
                return typeof(Guid);
            default:
                throw new ArgumentOutOfRangeException(nameof(fieldType), fieldType, "Unknown FieldType");
        }
    }

    private static DataTable CreateDataTable<T>()
    {
        var entityType = typeof(T);
        var table = new DataTable(entityType.Name);
        var properties = TypeDescriptor.GetProperties(entityType);
        foreach (PropertyDescriptor prop in properties)
        {
            var type = Nullable.GetUnderlyingType(prop.PropertyType) != null ? typeof(string) : prop.PropertyType;
            var dataColumn = new DataColumn(prop.Name, type);
 
            var propertyInfo = prop.ComponentType.GetProperty(prop.Name);

            dataColumn.Caption = propertyInfo.GetDisplayName();
     
   
            table.Columns.Add(dataColumn);
          
        }
        return table;
    }
}