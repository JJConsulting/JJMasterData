using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.Protheus.Abstractions;

namespace JJMasterData.Protheus;

public class ProtheusExpressionProvider(IProtheusService protheusService) : IAsyncExpressionProvider
{
    public string Prefix => "protheus";
    public string Title => "URL Protheus";
    public Guid? ConnectionId { get; set; }
    public async ValueTask<object?> EvaluateAsync(string expression, Dictionary<string, object?> parsedValues)
    {
        var exp = expression.Replace("\"", "").Replace("'", "").Split(',');
        if (exp.Length < 3)
            throw new JJMasterDataException("Invalid Protheus Request");

        var urlProtheus = ExpressionHelper.ReplaceExpression(exp[0], parsedValues);
        var functionName = ExpressionHelper.ReplaceExpression(exp[1], parsedValues);
        var parms = "";
        if (exp.Length >= 3)
            parms = ExpressionHelper.ReplaceExpression(exp[2], parsedValues);

        return await protheusService.CallFunctionAsync(urlProtheus, functionName, parms);
    }


}