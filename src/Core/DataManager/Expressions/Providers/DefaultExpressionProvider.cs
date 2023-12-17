#nullable enable
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataManager.Expressions.Abstractions;

namespace JJMasterData.Core.DataManager.Expressions.Providers;

public class DefaultExpressionProvider : IBooleanExpressionProvider, IAsyncExpressionProvider
{
    public string Prefix => "exp";
    public string Title => "Expression";
    private static object EvaluateObject(string replacedExpression)
    {
        using var dt = new DataTable();
        var result = dt.Compute(replacedExpression, string.Empty).ToString();
        return result!;
    }

    public bool Evaluate(string expression, IDictionary<string, object?> parsedValues)
    {
        var replacedExpression = ExpressionHelper.ReplaceExpression(expression, parsedValues);
        return StringManager.ParseBool(EvaluateObject(replacedExpression));
    }

    public Task<object?> EvaluateAsync(string expression, IDictionary<string, object?> parsedValues)
    {
        var replacedExpression = ExpressionHelper.ReplaceExpression(expression, parsedValues);
        try
        {
            return Task.FromResult<object?>(EvaluateObject(replacedExpression));
        }
        catch (Exception ex)
        {
            var error = new StringBuilder();
            error.AppendLine("Unhandled exception at a expression provider");
            error.AppendLine("Expression:");
            error.AppendLine(expression);
            error.AppendLine("Replaced Expression:");
            error.AppendLine(replacedExpression);
            error.AppendLine("Error Message:");
            error.AppendLine(ex.Message);
            throw new ExpressionException(error.ToString(), ex)
            {
                Expression = expression
            };
        }

    }
}