using System.Data;
using System.Threading.Tasks;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.Core.DataManager.Models;

namespace JJMasterData.Core.DataManager.Expressions.Providers;

public class InMemoryExpressionProvider : IExpressionProvider
{
    private readonly IExpressionParser _expressionParser;

    public InMemoryExpressionProvider(IExpressionParser expressionParser)
    {
        _expressionParser = expressionParser;
    }

    public bool CanHandle(string expressionType) => expressionType == "exp";

    public async Task<object> EvaluateAsync(string expression, FormStateData formStateData)
    {
        var parsedExpression = _expressionParser.ParseExpression(expression, formStateData, true);

        using var dt = new DataTable();
        var result = dt.Compute(parsedExpression, string.Empty).ToString()!;

        return await Task.FromResult(result);

    }
}