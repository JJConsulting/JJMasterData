using JJMasterData.Commons.Data.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using Xunit;

namespace JJMasterData.Commons.Tests;

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
        DataSource dataSource = new DataSource(dataTable);

        // Assert
        Assert.NotNull(dataSource.Data);
        Assert.Equal(2, dataSource.Data.Count);
        Assert.Equal(1, dataSource.Data[0]["Id"]);
        Assert.Equal("John", dataSource.Data[0]["Name"]);
        Assert.Equal(2, dataSource.Data[1]["Id"]);
        Assert.Equal("Jane", dataSource.Data[1]["Name"]);
    }

    [Fact]
    public void Constructor_DataTable_NullDataTable_ThrowsArgumentNullException()
    {
        DataTable? dt = null;
        Assert.Throws<ArgumentNullException>(() => new DataSource(dt!));
    }

    [Fact]
    public void Constructor_ListData_ValidData()
    {
        // Arrange
        var listData = new List<IDictionary<string, dynamic?>>()
        {
            new Dictionary<string, dynamic?>() { { "Id", 1 }, { "Name", "John" } },
            new Dictionary<string, dynamic?>() { { "Id", 2 }, { "Name", "Jane" } }
        };

        // Act
        DataSource dataSource = new DataSource(listData);

        // Assert
        Assert.NotNull(dataSource.Data);
        Assert.Equal(2, dataSource.Data.Count);
        Assert.Equal(1, dataSource.Data[0]["Id"]);
        Assert.Equal("John", dataSource.Data[0]["Name"]);
        Assert.Equal(2, dataSource.Data[1]["Id"]);
        Assert.Equal("Jane", dataSource.Data[1]["Name"]);
    }

    [Fact]
    public void ImplicitConversion_From_DataTable_ValidData()
    {
        // Arrange
        DataTable dataTable = new DataTable();
        dataTable.Columns.Add("Id", typeof(int));
        dataTable.Columns.Add("Name", typeof(string));

        // Add some test data
        dataTable.Rows.Add(1, "John");
        dataTable.Rows.Add(2, "Jane");

        // Act
        DataSource dataSource = dataTable;

        // Assert
        Assert.NotNull(dataSource.Data);
        Assert.Equal(2, dataSource.Data.Count);
        Assert.Equal(1, dataSource.Data[0]["Id"]);
        Assert.Equal("John", dataSource.Data[0]["Name"]);
        Assert.Equal(2, dataSource.Data[1]["Id"]);
        Assert.Equal("Jane", dataSource.Data[1]["Name"]);
    }

    [Fact]
    public void ImplicitConversion_From_ListData_ValidData()
    {
        // Arrange
        var listData = new List<IDictionary<string, dynamic?>>()
        {
            new Dictionary<string, dynamic?>() { { "Id", 1 }, { "Name", "John" } },
            new Dictionary<string, dynamic?>() { { "Id", 2 }, { "Name", "Jane" } }
        };

        // Act
        DataSource dataSource = listData;

        // Assert
        Assert.NotNull(dataSource.Data);
        Assert.Equal(2, dataSource.Data.Count);
        Assert.Equal(1, dataSource.Data[0]["Id"]);
        Assert.Equal("John", dataSource.Data[0]["Name"]);
        Assert.Equal(2, dataSource.Data[1]["Id"]);
        Assert.Equal("Jane", dataSource.Data[1]["Name"]);
    }
    
    [Fact]
    public void DataSourceToList_WithValidDataSource_ReturnsListData()
    {
        // Arrange
        var data = new List<IDictionary<string, dynamic?>>()
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
        var dataSource = new DataSource(data);

        // Act
        var result = (List<IDictionary<string, dynamic?>>)dataSource;

        // Assert
        Assert.Equal(data, result);
    }

    [Fact]
    public void DataSourceToDataTable_WithValidDataSource_ReturnsDataTable()
    {
        // Arrange
        var data = new List<IDictionary<string, dynamic?>>()
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
        var dataSource = new DataSource(data);

        // Act
        var result = (DataTable)dataSource;

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