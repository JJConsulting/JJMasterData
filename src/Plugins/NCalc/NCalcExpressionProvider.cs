using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.NCalc.Configuration;
using Microsoft.Extensions.Options;
using NCalc.Factories;

namespace JJMasterData.NCalc;

public sealed class NCalcExpressionProvider(
    IExpressionFactory expressionFactory,
    IOptionsSnapshot<NCalcExpressionProviderOptions> options) :
    IAsyncExpressionProvider,
    ISyncExpressionProvider
{
    public string Prefix => Options.ReplaceDefaultExpressionProvider ? "exp" : "ncalc";
    public string Title => Options.ReplaceDefaultExpressionProvider ? "Expression" : "NCalc";
    private NCalcExpressionProviderOptions Options { get; } = options.Value;
    
    public object? Evaluate(string expression, Dictionary<string, object?> parsedValues)
    {
        var replacedExpression = ExpressionHelper.ReplaceExpression(expression, parsedValues);
        var ncalcExpression = expressionFactory.Create(replacedExpression, Options.ExpressionOptions);
    
        foreach (var function in Options.AdditionalFunctions)
            ncalcExpression.EvaluateFunction += function;
        
        return ncalcExpression.Evaluate();
    }

    public ValueTask<object?> EvaluateAsync(string expression, Dictionary<string, object?> parsedValues) =>
        new(Evaluate(expression, parsedValues));
}