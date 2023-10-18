#nullable enable
using System.Threading.Tasks;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.Core.DataManager.Models;

namespace JJMasterData.Core.DataManager.Expressions.Providers;

internal class ValueExpressionProvider : IAsyncExpressionProvider, IBooleanExpressionProvider
{
    private readonly ExpressionParser _expressionParser;

    public ValueExpressionProvider(ExpressionParser expressionParser)
    {
        _expressionParser = expressionParser;
    }

    public string Prefix => "val";
    public string Title => "Value";

    private object? EvalutateObject(string expression, FormStateData formStateData)
    {
        if (expression.Contains("{"))
            return _expressionParser.ParseExpression(expression, formStateData);
        
        return expression.Replace("val:", string.Empty).Trim();
    }
    
    public bool Evaluate(string expression, FormStateData formStateData)
    {
        return StringManager.ParseBool(EvalutateObject(expression,formStateData));
    }

    public async Task<object?> EvaluateAsync(string expression, FormStateData formStateData)
    {
        return await Task.FromResult(EvalutateObject(expression, formStateData));
    }
}