#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using Microsoft.Extensions.Options;
using NCalc.Factories;

namespace JJMasterData.Core.DataManager.Expressions.Providers;

public sealed class DefaultExpressionProvider(
    IExpressionFactory expressionFactory,
    IAsyncExpressionFactory asyncExpressionFactory,
    IServiceProvider serviceProvider,
    IOptions<MasterDataCoreOptions> options) : ISyncExpressionProvider, IAsyncExpressionProvider
{
    public string Prefix => "exp";
    public string Title => "Expression";
    
    public Guid? ConnectionId { get; set; }
    
    public object? Evaluate(string expression, Dictionary<string, object?> parsedValues)
    {
        var replacedExpression = ExpressionHelper.ReplaceExpression(expression, parsedValues);
        var ncalcExpression = expressionFactory.Create(replacedExpression, options.Value.ExpressionContext with
        {
            StaticParameters = new Dictionary<string, object?>
            {
                {"serviceProvider", serviceProvider}
            }
        });
        return ncalcExpression.Evaluate();
    }

    public ValueTask<object?> EvaluateAsync(string expression, Dictionary<string, object?> parsedValues)
    {
        var replacedExpression = ExpressionHelper.ReplaceExpression(expression, parsedValues);
        var ncalcExpression = asyncExpressionFactory.Create(replacedExpression, options.Value.AsyncExpressionsContext with
        {
            StaticParameters = new Dictionary<string, object?>
            {
                {"serviceProvider", serviceProvider},
                {"connectionId", ConnectionId}
            }
        });
        return ncalcExpression.EvaluateAsync();
    }
}