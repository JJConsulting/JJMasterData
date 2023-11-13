using JJMasterData.Core.DataManager.Expressions;
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
        var replacedExpression = ExpressionHelper.ReplaceExpression(expression, parsedValues);
        var ncalcExpression = new Expression(replacedExpression, EvaluateOptions.IgnoreCase);
        return (bool)ncalcExpression.Evaluate();
    }

    public async Task<object?> EvaluateAsync(string expression, IDictionary<string, object?> parsedValues)
    {
        var replacedExpression = ExpressionHelper.ReplaceExpression(expression, parsedValues);
        var ncalcAsyncExpression = new AsyncExpression(replacedExpression, NCalcAsync.EvaluateOptions.IgnoreCase);
        return await ncalcAsyncExpression.EvaluateAsync();
    }
}