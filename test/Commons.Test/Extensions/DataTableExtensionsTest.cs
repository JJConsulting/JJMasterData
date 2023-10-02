using System.Data;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Extensions;

namespace JJMasterData.Commons.Test.Extensions;

public class DataTableExtensionsTest
{
    [Fact]
    public void ToModelList_Test()
    {
        var dataTable = new DataTable();

        var dataColumn = new DataColumn
        {
            DataType = typeof(string),
            ColumnName = "Name"
        };
        dataTable.Columns.Add(dataColumn);

        var dataRow1 = dataTable.NewRow();
        var dataRow2 = dataTable.NewRow();

        dataRow1["Name"] = "Test1";
        dataRow2["Name"] = "Test2";

        dataTable.Rows.Add(dataRow1);
        dataTable.Rows.Add(dataRow2);

        var modelList = dataTable.ToModelList<Element>();

        Assert.True(modelList?.Count == 2);
    }
}