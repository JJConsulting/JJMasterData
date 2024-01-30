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
    
    private static readonly DataTable _expressionsDataTable = new();
    
    private static object EvaluateObject(string replacedExpression)
    {
        var result = _expressionsDataTable.Compute(replacedExpression, string.Empty);
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
            error.AppendLine(ex.Message);
            error.AppendLine("Expression:");
            error.AppendLine(expression);
            error.AppendLine("Replaced Expression:");
            error.AppendLine(replacedExpression);

            throw new ExpressionException(error.ToString(), ex);
        }

    }
}