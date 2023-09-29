using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Web.Http.Abstractions;
using Moq;

namespace JJMasterData.Core.Test.Expressions;


public class ExpressionParserTests
{
    private ExpressionParser ExpressionParser { get; }

    public ExpressionParserTests()
    {
        var request = new Mock<IHttpRequest>();
        var session = new Mock<IHttpSession>();
        ExpressionParser = new ExpressionParser(request.Object,session.Object);
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