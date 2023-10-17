using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Logging;
using Moq;

namespace JJMasterData.Core.Test.DataManager.Expressions;


public class ExpressionParserTests
{
    private ExpressionParser ExpressionParser { get; }

    public ExpressionParserTests()
    {
        var httpContext = new Mock<IHttpContext>();
        var logger = new Mock<ILogger<ExpressionParser>>();
        ExpressionParser = new ExpressionParser(httpContext.Object,logger.Object);
    }
    
    [Fact]
    public void ParseExpression_NoIntervalSpecified_ReplacesFields()
    {
        // Arrange
        var request = new Mock<IHttpRequest>();
        request.Setup(r => r["componentName"]).Returns("ComponentNameValue");
        var formStateData = new FormStateData
        {
            UserValues = new Dictionary<string, object?>
            {
                {
                    "userField", "UserFieldValue"
                }
            },
            Values = new Dictionary<string, object?>()
            {
                {"formValue","FormValue"}
            },
            PageState = (PageState)0
        };

        // Act
        var result = ExpressionParser.ParseExpression("Test:{userField} {formValue}", formStateData, true);

        // Assert
        Assert.Equal("'UserFieldValue' 'FormValue'", result);
    }

    [Fact]
    public void ParseExpression_WithIntervalSpecified_ReplacesFields()
    {
        // Arrange
        var request = new Mock<IHttpRequest>();
        request.Setup(r => r["componentName"]).Returns("ComponentNameValue");
        var formStateData = new FormStateData
        {
            UserValues = new Dictionary<string, object?>
            {
                {
                    "userField", "UserFieldValue"
                }
            },
            Values = new Dictionary<string, object?>()
            {
                {"formValue","FormValue"}
            },
            PageState = (PageState)0
        };

        // Act
        var result = ExpressionParser.ParseExpression("Test:[userField] [formValue]", formStateData, false, new ExpressionParserInterval()
        {
            Begin = '[',
            End = ']'
        });

        // Assert
        Assert.Equal("UserFieldValue FormValue", result);
    }
}