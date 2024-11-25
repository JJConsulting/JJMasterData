using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository;

namespace JJMasterData.Commons.Test.Data.Entity;

public class OrderByDataTest
{
    [Fact]
    public void Validate_ShouldReturnTrue_WhenFieldsAreEmpty()
    {
        var orderByData = new OrderByData();
        var elementFields = new ElementFieldList([]);

        var result = orderByData.Validate(elementFields);

        Assert.True(result);
    }

    [Fact]
    public void Validate_ShouldReturnTrue_WhenAllFieldsExistInElementFields()
    {
        var orderByData = new OrderByData();
        orderByData.AddOrReplace("Name", OrderByDirection.Asc);
        orderByData.AddOrReplace("Date", OrderByDirection.Desc);

        var elementFields = new ElementFieldList([
            new()
            {
                Name = "Name",
            },
            new()
            {
                Name = "Date"
            }
        ]);

        var result = orderByData.Validate(elementFields);

        Assert.True(result);
    }

    [Fact]
    public void Validate_ShouldReturnFalse_WhenSomeFieldsDoNotExistInElementFields()
    {
        var orderByData = new OrderByData();
        orderByData.AddOrReplace("Name", OrderByDirection.Asc);
        orderByData.AddOrReplace("Date", OrderByDirection.Desc);

        var elementFields = new ElementFieldList([
            new()
            {
                Name = "Name",
            }
        ]);

        var result = orderByData.Validate(elementFields);

        Assert.False(result);
    }
}