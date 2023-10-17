using System.Data;
using System.Threading.Tasks;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.Core.DataManager.Models;

namespace JJMasterData.Core.DataManager.Expressions.Providers;

public class InMemoryExpressionProvider : IExpressionProvider
{
    private readonly ExpressionParser _expressionParser;

    public InMemoryExpressionProvider(ExpressionParser expressionParser)
    {
        _expressionParser = expressionParser;
    }
    
    public string Prefix => "exp";
    public string Title => "Expression";
    
    public async Task<object> EvaluateAsync(string expression, FormStateData formStateData)
    {
        var parsedExpression = _expressionParser.ParseExpression(expression, formStateData, true);

        using var dt = new DataTable();
        var result = dt.Compute(parsedExpression, string.Empty).ToString()!;

        return await Task.FromResult(result);

    }
}