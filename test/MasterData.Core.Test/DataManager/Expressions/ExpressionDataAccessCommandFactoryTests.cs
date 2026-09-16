using System.Data;
using JJMasterData.Core.DataManager.Expressions;

namespace JJMasterData.Core.Test.DataManager.Expressions;

public class ExpressionDataAccessCommandFactoryTests
{
    [Fact]
    public void Create_WithQuotedNullValue_ShouldCreateEmptyStringParameter()
    {
        var values = new Dictionary<string, object?>(StringComparer.InvariantCultureIgnoreCase)
        {
            ["searchid"] = null
        };

        var command = ExpressionDataAccessCommandFactory.Create(
            "SELECT * FROM Test WHERE Id = '{searchid}'",
            values);

        var parameter = Assert.Single(command.Parameters);
        Assert.Equal("@searchid", parameter.Name);
        Assert.Equal(string.Empty, parameter.Value);
        Assert.Equal(DbType.AnsiString, parameter.Type);
    }

    [Fact]
    public void Create_WithUnquotedNullValue_ShouldKeepNullParameter()
    {
        var values = new Dictionary<string, object?>(StringComparer.InvariantCultureIgnoreCase)
        {
            ["searchid"] = null
        };

        var command = ExpressionDataAccessCommandFactory.Create(
            "SELECT * FROM Test WHERE Id = {searchid}",
            values);

        var parameter = Assert.Single(command.Parameters);
        Assert.Equal("@searchid", parameter.Name);
        Assert.Null(parameter.Value);
        Assert.Equal(DbType.AnsiString, parameter.Type);
    }
}
