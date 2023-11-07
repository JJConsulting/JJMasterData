using JJMasterData.Core.DataManager.Expressions.Abstractions;
using NCalc;
using AsyncExpression = NCalcAsync.Expression;

namespace JJMasterData.NCalc;

public class NCalcExpressionProvider : IAsyncExpressionProvider, IBooleanExpressionProvider
{
    public string Prefix => "ncalc";
    public string Title => "NCalc";
    
    public bool Evaluate(string expression, IDictionary<string, object?> parsedValues)
    {
        var ncalcExpression = new Expression(expression)
        {
            Parameters = parsedValues as Dictionary<string,object>
        };
        return (bool)ncalcExpression.Evaluate();
    }

    public async Task<object?> EvaluateAsync(string expression, IDictionary<string, object?> parsedValues)
    {
        var asyncExpression = new AsyncExpression(expression)
        {
            Parameters = parsedValues as Dictionary<string, object>
        };

        return await asyncExpression.EvaluateAsync();
    }
}