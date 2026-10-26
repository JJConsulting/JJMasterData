using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository;

namespace JJMasterData.Commons.Test.Data.Entity;

public class OrderByDataTest
{
    [Theory]
    [InlineData(null, "TenantId ASC,Id ASC")]
    [InlineData("Name DESC", "Name DESC,TenantId ASC,Id ASC")]
    [InlineData("Id DESC,Name ASC", "Id DESC,Name ASC,TenantId ASC")]
    [InlineData("tenantid DESC,id DESC", "tenantid DESC,id DESC")]
    public void AddIfMissingPreservesExistingOrderAndAppendsMissingKeyFields(string? original, string expected)
    {
        var orderBy = OrderByData.FromString(original);

        orderBy.AddIfMissing("TenantId", OrderByDirection.Asc)
            .AddIfMissing("Id", OrderByDirection.Asc);

        Assert.Equal(expected, orderBy.ToQueryParameter());
    }

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
