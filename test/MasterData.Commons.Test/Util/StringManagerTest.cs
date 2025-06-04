using JJMasterData.Commons.Util;

namespace JJMasterData.Commons.Test.Util;

public class StringManagerTest
{
    [Theory]
    [InlineData("{apple} [banana] {cherry} [grape]", '{', '}', new[] { "apple", "cherry" })]
    [InlineData("|apple| [banana] |cherry| [grape]", '|', '|', new[] { "apple", "cherry" })]
    [InlineData("|apple| [banana] |cherry| [grape]", '{', '}', new string[] { })]
    [InlineData("{IsInsert} || {IsUpdate}", '{', '}', new[] { "IsInsert", "IsUpdate"})]
    [InlineData("'{PageState}' == 'Insert'", '{', '}', new[] { "PageState"})]
    public void FindValuesByInterval_ReturnsCorrectValues(string text, char begin, char end, string[] expected)
    {
        // Act
        var result = StringManager.FindValuesByInterval(text, begin, end);

        // Assert
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("true", true)]
    [InlineData("True", true)]
    [InlineData("TRUE", true)]
    [InlineData("false", false)]
    [InlineData("1", true)]
    [InlineData("0", false)]
    [InlineData("randomString", false)]
    [InlineData("", false)]
    [InlineData("10", false)]
    [InlineData(null, false)]
    public void ParseBool_ReturnsExpectedResult(string? input, bool expected)
    {
        var result = StringManager.ParseBool(input);
        Assert.Equal(expected, result);
    }
}