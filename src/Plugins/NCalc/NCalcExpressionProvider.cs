using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.NCalc.Configuration;
using Microsoft.Extensions.Options;
using NCalc;

namespace JJMasterData.NCalc;

public class NCalcExpressionProvider(IOptions<NCalcExpressionProviderOptions> options) : IAsyncExpressionProvider, IBooleanExpressionProvider
{
    public string Prefix => Options.ReplaceDefaultExpressionProvider ? "exp" : "ncalc";
    public string Title => Options.ReplaceDefaultExpressionProvider ? "Expression" : "NCalc";
    private NCalcExpressionProviderOptions Options { get; } = options.Value;
    
    public bool Evaluate(string expression, IDictionary<string, object?> parsedValues)
    {
        return (bool)(ExecuteNCalcExpression(expression, parsedValues) ?? false);
    }

    private object? ExecuteNCalcExpression(string expression, IDictionary<string, object?> parsedValues)
    {
        var replacedExpression = ExpressionHelper.ReplaceExpression(expression, parsedValues);
        var ncalcExpression = new Expression(replacedExpression, Options.EvaluateOptions);

        foreach (var function in Options.AdditionalFunctions)
            ncalcExpression.EvaluateFunction += function;
        
        return ncalcExpression.Evaluate();
    }

    public Task<object?> EvaluateAsync(string expression, IDictionary<string, object?> parsedValues)
    {
        return Task.FromResult(ExecuteNCalcExpression(expression,parsedValues));
    }
}