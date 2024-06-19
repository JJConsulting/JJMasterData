using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Logging;
using Moq;

namespace JJMasterData.Core.Test.DataManager.Expressions;

public class ExpressionsServiceTests
{
    private readonly ExpressionsService _expressionsService;
    private readonly Mock<ILogger<ExpressionsService>> _loggerMock = new();
    private readonly Mock<IAsyncExpressionProvider> _expressionAsyncProviderMock = new();
    private readonly Mock<ISyncExpressionProvider> _expressionBooleanProviderMock = new();
    private readonly Mock<IEncryptionService> _encryptionServiceMock = new();

    private static IHttpContext MockHttpContext()
    {
        var mockHttpContext = new Mock<IHttpContext>();
        return mockHttpContext.Object;
    }

    private static ILogger<ExpressionParser> MockLogger()
    {
        return new Mock<ILogger<ExpressionParser>>().Object;
    }
    
    public ExpressionsServiceTests()
    {
        _expressionAsyncProviderMock.SetupGet(p => p.Prefix).Returns("example");
        _expressionBooleanProviderMock.SetupGet(p => p.Prefix).Returns("bool_example");
        _expressionsService = new ExpressionsService(
            new List<IExpressionProvider>
            {
                _expressionAsyncProviderMock.Object,
                _expressionBooleanProviderMock.Object
            },
             new ExpressionParser(MockHttpContext(), MockLogger()),
            _encryptionServiceMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task GetDefaultValueAsync_ShouldReturnExpressionValue()
    {
        // Arrange
        var field = new FormElementFieldSelector(new FormElement
        {
            Name = "field",
            Fields =
            {
                new FormElementField { TriggerExpression = "example:sampleExpression" }
            }
        }, "field");

        var values = new Dictionary<string, object?>();
        
        var formStateData = new FormStateData(
            values, 
            null,
            new PageState()
        );
        
        _expressionAsyncProviderMock.Setup(p => p.EvaluateAsync("example:sampleExpression", values))
            .ReturnsAsync("ExpressionResult");

        // Act
        var result = await _expressionsService.GetDefaultValueAsync(field, formStateData);

        // Assert
        Assert.Equal("ExpressionResult", result);
    }

    [Fact]
    public bool GetBoolValue_ShouldReturnBooleanValue()
    {
        // Arrange
        const string expression = "bool_example:sampleExpression";
        var values = new Dictionary<string, object?>();
        
        var formStateData = new FormStateData(
            values, 
            null,
            new PageState()
        );

        _expressionBooleanProviderMock.Setup(p => p.Evaluate("bool_example:sampleExpression", values))
            .Returns(true);

        // Act
        var result = _expressionsService.GetBoolValue(expression, formStateData);

        // Assert
        Assert.True(result);

        return result;
    }

    [Fact]
    public async Task GetTriggerValueAsync_ShouldReturnExpressionValue()
    {
        // Arrange
        var field = new FormElementFieldSelector(new FormElement
        {
            Name = "field",
            Fields =
            {
                new FormElementField { TriggerExpression = "example:sampleExpression" }
            }
        }, "field");
        var values = new Dictionary<string, object?>();
        var formStateData = new FormStateData(
            new Dictionary<string, object?>(), 
            values,
            new PageState()
        );
        
        _expressionAsyncProviderMock.Setup(p => p.EvaluateAsync("example:sampleExpression", values))
            .ReturnsAsync("TriggerExpressionResult");

        // Act
        var result = await _expressionsService.GetTriggerValueAsync(field, formStateData);

        // Assert
        Assert.Equal("TriggerExpressionResult", result);
    }
    
}
