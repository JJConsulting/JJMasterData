using System.Text;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace JJMasterData.Core.Test.DataManager.Expressions;

public class ExpressionParserTests
{
    [Fact]
    public void ParseExpression_WithNullExpression_ShouldReturnEmptyDictionary()
    {
        // Arrange
        var parser = new ExpressionParser(MockHttpContext(), MockMasterDataUser(),MockLogger());

        // Act
        var result = parser.ParseExpression(null, new FormStateData(PageState.List));

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ParseExpression_WithFieldInUserValues_ShouldReturnExpectedValue()
    {
        // Arrange
        var httpContext = MockHttpContext();
        var userValues = new Dictionary<string, object?> { { "Name", "Gustavo" } };
        var formStateData = new FormStateData(new(), userValues, PageState.List);
        var parser = new ExpressionParser(httpContext, MockMasterDataUser(),MockLogger());

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
        var parser = new ExpressionParser(MockHttpContext(), MockMasterDataUser(),MockLogger());

        // Act
        var result = parser.ParseExpression("{UnknownField}", new FormStateData(PageState.List));

        // Assert
        Assert.Single(result);
        Assert.Null(result["UnknownField"]);
    }

    [Fact]
    public void ParseExpression_WithSessionField_ShouldKeepValueAfterRequestEnds()
    {
        var sessionValue = Encoding.UTF8.GetBytes("BU-SESSION");
        var session = new Mock<ISession>();
        session.SetupGet(value => value.IsAvailable).Returns(true);
        session
            .Setup(value => value.TryGetValue("UNID_NEG", out sessionValue))
            .Returns(true);
        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { Session = session.Object }
        };
        var parser = new ExpressionParser(httpContextAccessor, MockMasterDataUser(), MockLogger());
        var formStateData = new FormStateData(PageState.Import);

        var firstRow = parser.ParseExpression("{UNID_NEG}", formStateData);
        httpContextAccessor.HttpContext = null;
        var remainingRows = Enumerable.Range(0, 3)
            .Select(_ => parser.ParseExpression("{UNID_NEG}", formStateData))
            .ToList();

        Assert.Equal("BU-SESSION", firstRow["UNID_NEG"]);
        Assert.All(remainingRows, row => Assert.Equal("BU-SESSION", row["UNID_NEG"]));
    }


    private static IHttpContextAccessor MockHttpContext()
    {
        var mockHttpContext = new Mock<IHttpContextAccessor>();
        return mockHttpContext.Object;
    }

    private static ILogger<ExpressionParser> MockLogger()
    {
        return new Mock<ILogger<ExpressionParser>>().Object;
    }
    private static IMasterDataUser MockMasterDataUser()
    {
        var mockHttpContext = new Mock<IMasterDataUser>();
        return mockHttpContext.Object;
    }
}
