#nullable enable
using System.Collections.Generic;

namespace JJMasterData.Core.DataManager.Expressions;

public static class ExpressionHelper
{
    public const char Begin = '{';
    public const char End = '}';

    public static string ReplaceExpression(string expression, IDictionary<string,object?> values)
    {
        foreach (var kvp in values)
        {
            var value = kvp.Value?.ToString();
            expression = expression.Replace($"{Begin}{kvp.Key}{End}", value);
        }

        return expression;
    }
}