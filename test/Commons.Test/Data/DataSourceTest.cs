using System.Data;
using JJMasterData.Commons.Data.Entity.Repository;

namespace JJMasterData.Commons.Test.Data;

public class DataSourceTest
{
    [Fact]
    public void Constructor_DataTable_ValidData()
    {
        // Arrange
        DataTable dataTable = new DataTable();
        dataTable.Columns.Add("Id", typeof(int));
        dataTable.Columns.Add("Name", typeof(string));

        // Add some test data
        dataTable.Rows.Add(1, "John");
        dataTable.Rows.Add(2, "Jane");

        // Act
        DictionaryListResult dataSource = DictionaryListResult.FromDataTable(dataTable);

        // Assert
        Assert.NotNull(dataSource.Data);
        Assert.Equal(2, dataSource.Data.Count);
        Assert.Equal(1, dataSource.Data[0]["Id"]);
        Assert.Equal("John", dataSource.Data[0]["Name"]);
        Assert.Equal(2, dataSource.Data[1]["Id"]);
        Assert.Equal("Jane", dataSource.Data[1]["Name"]);
    }

    [Fact]
    public void Constructor_ListData_ValidData()
    {
        // Arrange
        var listData = new List<Dictionary<string, dynamic?>>()
        {
            new Dictionary<string, dynamic?>() { { "Id", 1 }, { "Name", "John" } },
            new Dictionary<string, dynamic?>() { { "Id", 2 }, { "Name", "Jane" } }
        };

        // Act
        var dataSource = new DictionaryListResult(listData,listData.Count);

        // Assert
        Assert.NotNull(dataSource.Data);
        Assert.Equal(2, dataSource.Data.Count);
        Assert.Equal(1, dataSource.Data[0]["Id"]);
        Assert.Equal("John", dataSource.Data[0]["Name"]);
        Assert.Equal(2, dataSource.Data[1]["Id"]);
        Assert.Equal("Jane", dataSource.Data[1]["Name"]);
    }

    [Fact]
    public void DataSourceToDataTable_ValidData()
    {
        // Arrange
        var data = new List<Dictionary<string, dynamic?>>()
        {
            new Dictionary<string, dynamic?>()
            {
                {"Id", 1},
                {"Name", "John"},
                {"Age", null}
            },
            new Dictionary<string, dynamic?>()
            {
                {"Id", 2},
                {"Name", "Jane"},
                {"Age", 30}
            }
        };
        var dataSource = new DictionaryListResult(data, data.Count);

        // Act
        var result = dataSource.ToDataTable();

        // Assert
        Assert.Equal(3, result.Columns.Count); // There are three columns: "Id", "Name", "Age"
        Assert.Equal(2, result.Rows.Count); // There are two rows of data
        Assert.Equal(1, result.Rows[0]["Id"]);
        Assert.Equal("John", result.Rows[0]["Name"]);
        Assert.Equal(DBNull.Value, result.Rows[0]["Age"]); // Age is null
        Assert.Equal(2, result.Rows[1]["Id"]);
        Assert.Equal("Jane", result.Rows[1]["Name"]);
        Assert.Equal(30, result.Rows[1]["Age"]);
    }
}