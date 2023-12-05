#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataManager.Expressions.Abstractions;

namespace JJMasterData.Core.DataManager.Expressions.Providers;

public class ValueExpressionProvider : IAsyncExpressionProvider, IBooleanExpressionProvider
{
    public string Prefix => "val";
    public string Title => "Value";

    private static object EvalutateObject(string expression, IDictionary<string,object?> parsedValues)
    {
        if (expression.Contains(ExpressionHelper.Begin.ToString()))
            return ExpressionHelper.ReplaceExpression(expression, parsedValues);

        return expression.Trim();
    }
    
    public bool Evaluate(string expression, IDictionary<string,object?> parsedValues)
    {
        return StringManager.ParseBool(EvalutateObject(expression,parsedValues));
    }

    public Task<object?> EvaluateAsync(string expression, IDictionary<string,object?> parsedValues)
    {
        return Task.FromResult<object?>(EvalutateObject(expression, parsedValues));
    }
}