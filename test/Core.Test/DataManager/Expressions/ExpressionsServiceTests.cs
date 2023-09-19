using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services;

namespace JJMasterData.Core.Test.Expressions;

using Commons.Data.Entity;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

public class ExpressionsServiceTests
{
    private readonly ExpressionsService _expressionsService;
    private readonly Mock<IExpressionParser> _expressionParserMock = new();
    private readonly Mock<ILogger<ExpressionsService>> _loggerMock = new();
    private readonly Mock<IExpressionProvider> _expressionProviderMock = new();

    public ExpressionsServiceTests()
    {
        _expressionsService = new ExpressionsService(
            new List<IExpressionProvider> { _expressionProviderMock.Object },
            _expressionParserMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task GetDefaultValueAsync_ShouldReturnExpressionValue()
    {
        // Arrange
        var field = new ElementField { DefaultValue = "sampleExpression" };
        var formStateData = new FormStateData(
            new Dictionary<string, object?>(), 
            null,
            new PageState()
        );

        _expressionProviderMock.Setup(p => p.CanHandle(It.IsAny<string>())).Returns(true);
        _expressionProviderMock.Setup(p => p.EvaluateAsync("sampleExpression", formStateData))
            .ReturnsAsync("ExpressionResult");

        // Act
        var result = await _expressionsService.GetDefaultValueAsync(field, formStateData);

        // Assert
        Assert.Equal("ExpressionResult", result);
    }

    [Fact]
    public void ParseExpression_ShouldCallExpressionParser()
    {
        // Arrange
        var expression = "sampleExpression";
        var formStateData = new FormStateData(
            new Dictionary<string, object?>(), 
            null,
            new PageState()
        );
        var quotationMarks = true;
        var interval = new ExpressionParserInterval();

        _expressionParserMock.Setup(p => p.ParseExpression(expression, formStateData, quotationMarks, interval))
            .Returns("ParsedExpression");

        // Act
        var result = _expressionsService.ParseExpression(expression, formStateData, quotationMarks, interval);

        // Assert
        Assert.Equal("ParsedExpression", result);
    }

    [Fact]
    public async Task GetBoolValueAsync_ShouldReturnBooleanValue()
    {
        // Arrange
        var expression = "sampleExpression";
        var formStateData = new FormStateData(
            new Dictionary<string, object?>(), 
            null,
            new PageState()
        );

        _expressionProviderMock.Setup(p => p.CanHandle(It.IsAny<string>())).Returns(true);
        _expressionProviderMock.Setup(p => p.EvaluateAsync("sampleExpression", formStateData))
            .ReturnsAsync("true");

        // Act
        var result = await _expressionsService.GetBoolValueAsync(expression, formStateData);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task GetTriggerValueAsync_ShouldReturnExpressionValue()
    {
        // Arrange
        var field = new FormElementField { TriggerExpression = "sampleExpression" };
        var formStateData = new FormStateData(
            new Dictionary<string, object?>(), 
            null,
            new PageState()
        );

        _expressionProviderMock.Setup(p => p.CanHandle(It.IsAny<string>())).Returns(true);
        _expressionProviderMock.Setup(p => p.EvaluateAsync("sampleExpression", formStateData))
            .ReturnsAsync("TriggerExpressionResult");

        // Act
        var result = await _expressionsService.GetTriggerValueAsync(field, formStateData);

        // Assert
        Assert.Equal("TriggerExpressionResult", result);
    }
    
}
