using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Services;

namespace JJMasterData.Commons.Protheus;

public class ProtheusExpressionProvider : IExpressionProvider
{
    private readonly IExpressionParser _expressionParser;
    private readonly IProtheusService _protheusService;

    public ProtheusExpressionProvider(IExpressionParser expressionParser, IProtheusService protheusService)
    {
        _expressionParser = expressionParser;
        _protheusService = protheusService;
    }

    public bool CanHandle(string expressionType) => expressionType == "protheus";

    public async Task<object> EvaluateAsync(string expression, FormStateData formStateData)
    {
        var exp = expression.Replace("\"", "").Replace("'", "").Split(',');
        if (exp.Length < 3)
            throw new JJMasterDataException("Invalid Protheus Request");

        var urlProtheus = _expressionParser.ParseExpression(exp[0], formStateData, false);
        var functionName = _expressionParser.ParseExpression(exp[1], formStateData, false);
        var parms = "";
        if (exp.Length >= 3)
            parms = _expressionParser.ParseExpression(exp[2], formStateData, false);

        return await _protheusService.CallFunctionAsync(urlProtheus, functionName, parms);
    }
}