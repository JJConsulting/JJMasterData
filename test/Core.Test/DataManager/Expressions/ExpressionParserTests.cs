using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Logging;
using Moq;

namespace JJMasterData.Core.Test.DataManager.Expressions;

public class ExpressionParserTests
{
    [Fact]
    public void ParseExpression_WithNullExpression_ShouldReturnEmptyDictionary()
    {
        // Arrange
        var parser = new ExpressionParser(MockHttpContext(), MockLogger());

        // Act
        var result = parser.ParseExpression(null, new FormStateData()
        {
            Values = new Dictionary<string, object>(),
            UserValues = new Dictionary<string, object>(),
            PageState = PageState.List
        });

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ParseExpression_WithFieldInUserValues_ShouldReturnExpectedValue()
    {
        // Arrange
        var httpContext = MockHttpContext();
        var userValues = new Dictionary<string, object> { { "Name", "Gustavo" } };
        var formStateData = new FormStateData { UserValues = userValues,  Values = new Dictionary<string, object>(), PageState = PageState.List };
        var parser = new ExpressionParser(httpContext, MockLogger());

        // Act
        var result = parser.ParseExpression("{Name}", formStateData);

        // Assert
        Assert.Single(result);
        Assert.Equal("Gustavo", result["Name"]);
    }


    [Fact]
    public void ParseExpression_WithUnknownField_ShouldReturnEmptyValue()
    {
        // Arrange
        var parser = new ExpressionParser(MockHttpContext(), MockLogger());

        // Act
        var result = parser.ParseExpression("{UnknownField}", new FormStateData()
        {
            Values = new Dictionary<string, object>(),
            UserValues = new Dictionary<string, object>(),
            PageState = PageState.List
        });

        // Assert
        Assert.Single(result);
        Assert.Empty(result["UnknownField"]!.ToString()!);
    }

    // Add more test cases to cover other scenarios
    // ...

    private IHttpContext MockHttpContext()
    {
        var mockHttpContext = new Mock<IHttpContext>();
        return mockHttpContext.Object;
    }

    private ILogger<ExpressionParser> MockLogger()
    {
        return new Mock<ILogger<ExpressionParser>>().Object;
    }
}