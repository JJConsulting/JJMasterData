#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Core.DataManager.Expressions.Abstractions;

namespace JJMasterData.Core.DataManager.Expressions.Providers;

public class ValueExpressionProvider : IAsyncExpressionProvider, ISyncExpressionProvider
{
    public string Prefix => "val";
    public string Title => "Value";

    public object Evaluate(string expression, Dictionary<string, object?> parsedValues)
    {
        if (expression.Contains(ExpressionHelper.Begin.ToString()))
            return ExpressionHelper.ReplaceExpression(expression, parsedValues).Trim();

        return expression.Trim();
    }

    public Task<object?> EvaluateAsync(string expression, Dictionary<string,object?> parsedValues)
        => Task.FromResult<object?>(Evaluate(expression, parsedValues));
}