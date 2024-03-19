#nullable enable
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace JJMasterData.Core.DataManager.Expressions;

public static class ExpressionHelper
{
    public const char Begin = '{';
    public const char End = '}';

    public static string ReplaceExpression(string expression, Dictionary<string, object?> values)
    {
        var stringBuilder = new StringBuilder(expression);

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

            stringBuilder.Replace($"{Begin}{kvp.Key}{End}", stringValue);
        }

        return stringBuilder.ToString();
    }
}