using System.Threading.Tasks;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Expressions.Abstractions;

namespace JJMasterData.Core.DataManager.Services;

public class ValueExpressionProvider : IExpressionProvider
{
    private readonly IExpressionParser _expressionParser;

    public ValueExpressionProvider(IExpressionParser expressionParser)
    {
        _expressionParser = expressionParser;
    }

    public bool CanHandle(string expressionType) => expressionType == "val";

    public async Task<object> EvaluateAsync(string expression, FormStateData formStateData)
    {
        if (expression.Contains("{"))
            return await Task.FromResult<object>(_expressionParser.ParseExpression(expression, formStateData, false)!);
        
        return await Task.FromResult<object>(expression.Replace("val:", "").Trim());
    }
}