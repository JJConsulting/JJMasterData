using System.Text.Json;
using JJMasterData.Core.DataManager.Expressions;



namespace JJMasterData.Core.Test.DataManager.Expressions;

public class ExpressionHelperTests
{
    [Theory]
    [InlineData("exp: {PRICE} - {RANGE}", "{\"PRICE\": 0, \"RANGE\": 0.01}","exp: 0 - 0.01")]
    [InlineData("exp: {PRICE} - {RANGE}", "{\"PRICE\": 123.33, \"RANGE\": 0.01}","exp: 123.33 - 0.01")]
    [InlineData("exp: {PRICE} - {RANGE}", "{\"PRICE\": null, \"RANGE\": 0.01}","exp:  - 0.01")]
    public void Expression_Replace_Test(string expression, string parsedValuesJson, string expected)
    {
        var parsedValues = JsonSerializer.Deserialize<Dictionary<string, object?>>(parsedValuesJson)!;

        var parsedExpression = ExpressionHelper.ReplaceExpression(expression, parsedValues);
     
        Assert.Equal(parsedExpression, expected);
    }
}