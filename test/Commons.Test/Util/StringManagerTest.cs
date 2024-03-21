using JJMasterData.Commons.Util;

namespace JJMasterData.Commons.Test.Util;

public class StringManagerTest
{
    [Fact]
    public void FindValuesByInterval_ReturnsCorrectValues()
    {
        // Arrange
        const string text = "{apple} [banana] {cherry} [date]";
        const char begin = '{';
        const char end = '}';

        // Act
        var result = StringManager.FindValuesByInterval(text, begin, end);

        // Assert
        Assert.Equal(new List<string> { "apple", "cherry" }, result);
    }
    [Fact]
    public void FindValuesByInterval_NoBeginEndChars()
    {
        // Arrange
        const string text = "apple banana cherry date";
        const char begin = '{';
        const char end = '}';

        // Act
        var result = StringManager.FindValuesByInterval(text, begin, end);

        // Assert
        Assert.Empty(result);
    }
}