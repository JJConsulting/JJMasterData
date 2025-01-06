using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace JJMasterData.Commons.Data.Extensions;

public static class DataExtensions
{
    public static async Task FillAsync(this DbDataReader dataReader, DataTable dataTable, CancellationToken cancellationToken = default)
    {
        for (var i = 0; i < dataReader.FieldCount; i++)
        {
            var column = new DataColumn(dataReader.GetName(i), dataReader.GetFieldType(i)!);
            
            if(!dataTable.Columns.Contains(column.ColumnName))
                dataTable.Columns.Add(column);
        }

        var values = new object[dataTable.Columns.Count];
        dataTable.BeginLoadData();
        while (await dataReader.ReadAsync(cancellationToken))
        {
            dataReader.GetValues(values);
            dataTable.LoadDataRow(values, true);
        }
        dataTable.EndLoadData();
    }
    
    public static async Task FillAsync(this DbDataReader dataReader,DataSet dataSet, CancellationToken cancellationToken = default)
    {
        var index = 1;
        do
        {
            var dataTable = dataSet.Tables.Add($"Result {index}");
            
            await dataReader.FillAsync(dataTable, cancellationToken);
            
            index++;
        } while (await dataReader.NextResultAsync(cancellationToken));
    }
}