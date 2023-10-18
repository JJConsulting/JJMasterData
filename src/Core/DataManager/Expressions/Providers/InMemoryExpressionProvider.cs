#nullable enable
using System.Data;
using System.Threading.Tasks;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.Core.DataManager.Models;

namespace JJMasterData.Core.DataManager.Expressions.Providers;

internal class InMemoryExpressionProvider : IBooleanExpressionProvider, IAsyncExpressionProvider
{
    private readonly ExpressionParser _expressionParser;

    public InMemoryExpressionProvider(ExpressionParser expressionParser)
    {
        _expressionParser = expressionParser;
    }
    
    public string Prefix => "exp";
    public string Title => "Expression";
    private object EvaluateObject(string expression, FormStateData formStateData)
    {
        var parsedExpression = _expressionParser.ParseExpression(expression, formStateData, true);

        using var dt = new DataTable();
        var result = dt.Compute(parsedExpression, string.Empty).ToString()!;

        return result;
    }

    public bool Evaluate(string expression, FormStateData formStateData)
    {
        return StringManager.ParseBool(EvaluateObject(expression, formStateData));
    }
    
    public async Task<object?> EvaluateAsync(string expression, FormStateData formStateData)
    {
        return await Task.FromResult(EvaluateObject(expression, formStateData));
    }
}