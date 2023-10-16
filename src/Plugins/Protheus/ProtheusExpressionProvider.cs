using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Protheus.Abstractions;

namespace JJMasterData.Protheus;

public class ProtheusExpressionProvider : IExpressionProvider
{
    private readonly ExpressionParser _expressionParser;
    private readonly IProtheusService _protheusService;

    public ProtheusExpressionProvider(ExpressionParser expressionParser, IProtheusService protheusService)
    {
        _expressionParser = expressionParser;
        _protheusService = protheusService;
    }

    public string Prefix => "protheus";
    
    public async Task<object> EvaluateAsync(string expression, FormStateData formStateData)
    {
        var exp = expression.Replace("\"", "").Replace("'", "").Split(',');
        if (exp.Length < 3)
            throw new JJMasterDataException("Invalid Protheus Request");

        var urlProtheus = _expressionParser.ParseExpression(exp[0], formStateData);
        var functionName = _expressionParser.ParseExpression(exp[1], formStateData);
        var parms = "";
        if (exp.Length >= 3)
            parms = _expressionParser.ParseExpression(exp[2], formStateData);

        return await _protheusService.CallFunctionAsync(urlProtheus!, functionName!, parms!);
    }
}