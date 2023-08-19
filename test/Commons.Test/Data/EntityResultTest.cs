using JJMasterData.Commons.Data.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using Xunit;

namespace JJMasterData.Commons.Tests;

public class EntityResultTest
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
        EntityResult entityResult = new EntityResult(dataTable,dataTable.Rows.Count);

        // Assert
        Assert.NotNull(entityResult.Data);
        Assert.Equal(2, entityResult.Data.Count);
        Assert.Equal(1, entityResult.Data[0]["Id"]);
        Assert.Equal("John", entityResult.Data[0]["Name"]);
        Assert.Equal(2, entityResult.Data[1]["Id"]);
        Assert.Equal("Jane", entityResult.Data[1]["Name"]);
    }

    [Fact]
    public void Constructor_DataTable_NullDataTable_ThrowsArgumentNullException()
    {
        DataTable? dt = null;
        Assert.Throws<ArgumentNullException>(() => new EntityResult(dt!,dt.Rows.Count));
    }

    [Fact]
    public void Constructor_ListData_ValidData()
    {
        // Arrange
        var listData = new List<IDictionary<string, object?>>()
        {
            new Dictionary<string, object?>() { { "Id", 1 }, { "Name", "John" } },
            new Dictionary<string, object?>() { { "Id", 2 }, { "Name", "Jane" } }
        };

        // Act
        EntityResult entityResult = new EntityResult(listData,listData.Count);

        // Assert
        Assert.NotNull(entityResult.Data);
        Assert.Equal(2, entityResult.Data.Count);
        Assert.Equal(1, entityResult.Data[0]["Id"]);
        Assert.Equal("John", entityResult.Data[0]["Name"]);
        Assert.Equal(2, entityResult.Data[1]["Id"]);
        Assert.Equal("Jane", entityResult.Data[1]["Name"]);
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
        EntityResult entityResult = dataTable;

        // Assert
        Assert.NotNull(entityResult.Data);
        Assert.Equal(2, entityResult.Data.Count);
        Assert.Equal(1, entityResult.Data[0]["Id"]);
        Assert.Equal("John", entityResult.Data[0]["Name"]);
        Assert.Equal(2, entityResult.Data[1]["Id"]);
        Assert.Equal("Jane", entityResult.Data[1]["Name"]);
    }

    [Fact]
    public void ImplicitConversion_From_ListData_ValidData()
    {
        // Arrange
        var listData = new List<IDictionary<string, object?>>()
        {
            new Dictionary<string, object?>() { { "Id", 1 }, { "Name", "John" } },
            new Dictionary<string, object?>() { { "Id", 2 }, { "Name", "Jane" } }
        };

        // Act
        EntityResult entityResult = listData;

        // Assert
        Assert.NotNull(entityResult.Data);
        Assert.Equal(2, entityResult.Data.Count);
        Assert.Equal(1, entityResult.Data[0]["Id"]);
        Assert.Equal("John", entityResult.Data[0]["Name"]);
        Assert.Equal(2, entityResult.Data[1]["Id"]);
        Assert.Equal("Jane", entityResult.Data[1]["Name"]);
    }
    
    [Fact]
    public void EntityResultToList_WithValidEntityResult_ReturnsListData()
    {
        // Arrange
        var data = new List<IDictionary<string, object?>>()
        {
            new Dictionary<string, object?>()
            {
                {"Id", 1},
                {"Name", "John"},
                {"Age", null}
            },
            new Dictionary<string, object?>()
            {
                {"Id", 2},
                {"Name", "Jane"},
                {"Age", 30}
            }
        };
        var entityResult = new EntityResult(data, data.Count);

        // Act
        var result = (List<IDictionary<string, object?>>)entityResult;

        // Assert
        Assert.Equal(data, result);
    }

    [Fact]
    public void EntityResultToDataTable_WithValidEntityResult_ReturnsDataTable()
    {
        // Arrange
        var data = new List<IDictionary<string, object?>>()
        {
            new Dictionary<string, object?>()
            {
                {"Id", 1},
                {"Name", "John"},
                {"Age", null}
            },
            new Dictionary<string, object?>()
            {
                {"Id", 2},
                {"Name", "Jane"},
                {"Age", 30}
            }
        };
        var entityResult = new EntityResult(data, data.Count);

        // Act
        var result = (DataTable)entityResult;

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