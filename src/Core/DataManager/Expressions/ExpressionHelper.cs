#nullable enable
using System.Collections.Generic;
using System.Globalization;

namespace JJMasterData.Core.DataManager.Expressions;

public static class ExpressionHelper
{
    public const char Begin = '{';
    public const char End = '}';

    public static string ReplaceExpression(string expression, IDictionary<string,object?> values)
    {
        foreach (var kvp in values)
        {
            var value = kvp.Value;
            
            var stringValue = value switch
            {
                double doubleValue => doubleValue.ToString("F", NumberFormatInfo.InvariantInfo),
                decimal decimalValue => decimalValue.ToString("F", NumberFormatInfo.InvariantInfo),
                float floatValue => floatValue.ToString("F", NumberFormatInfo.InvariantInfo),
                _ => value?.ToString() ?? string.Empty
            };

            expression = expression.Replace($"{Begin}{kvp.Key}{End}", stringValue);
        }

        return expression;
    }
}