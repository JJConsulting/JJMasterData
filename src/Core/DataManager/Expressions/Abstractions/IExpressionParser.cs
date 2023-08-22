#nullable enable
using JJMasterData.Core.DataManager.Services;

namespace JJMasterData.Core.DataManager.Expressions.Abstractions;

public interface IExpressionParser
{
    string? ParseExpression(
        string? expression,
        FormStateData formStateData,
        bool quotationMarks,
        ExpressionManagerInterval? interval = null);
}