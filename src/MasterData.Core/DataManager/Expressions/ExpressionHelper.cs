#nullable enable
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web;

namespace JJMasterData.Core.DataManager.Expressions;

public static class ExpressionHelper
{
    public const char Begin = '{';
    public const char End = '}';

    public static string ReplaceExpression(string expression, Dictionary<string, object?> values, bool encodeValue = false)
    {
        var stringBuilder = new StringBuilder(expression);

        foreach (var kvp in values)
        {
            var value = kvp.Value;

            var stringValue = value switch
            {
                double doubleValue => doubleValue.ToString("F6", NumberFormatInfo.InvariantInfo),
                decimal decimalValue => decimalValue.ToString("F6", NumberFormatInfo.InvariantInfo),
                float floatValue => floatValue.ToString("F6", NumberFormatInfo.InvariantInfo),
                _ => value?.ToString() ?? string.Empty
            };

            stringBuilder.Replace($"{Begin}{kvp.Key}{End}", encodeValue ? HttpUtility.HtmlEncode(stringValue) : stringValue);
        }

        return stringBuilder.ToString();
    }
}