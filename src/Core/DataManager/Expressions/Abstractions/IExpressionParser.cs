#nullable enable
using JJMasterData.Core.DataManager.Models;

namespace JJMasterData.Core.DataManager.Expressions.Abstractions;

public interface IExpressionParser
{
    string? ParseExpression(
        string? expression,
        FormStateData formStateData,
        bool addQuotationMarks = false,
        ExpressionParserInterval? interval = null);
}