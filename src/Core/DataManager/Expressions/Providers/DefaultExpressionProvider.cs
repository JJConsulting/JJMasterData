#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using Microsoft.Extensions.Options;
using NCalc;
using NCalc.Factories;

namespace JJMasterData.Core.DataManager.Expressions.Providers;

public sealed class DefaultExpressionProvider(
    IExpressionFactory expressionFactory,
    IServiceProvider serviceProvider,
    IOptions<MasterDataCoreOptions> options)
    : ISyncExpressionProvider, IAsyncExpressionProvider
{
    private readonly ExpressionContext _expressionContext = options.Value.ExpressionContext with
    {
        StaticParameters = new Dictionary<string, object?>(options.Value.ExpressionContext.StaticParameters)
        {
            ["ServiceProvider"] = serviceProvider
        }
    };

    public string Prefix => "exp";
    public string Title => "Expression";

    public Guid? ConnectionId { get; set; }
    
    public object? Evaluate(string expression, Dictionary<string, object?> parsedValues)
    {
        var replacedExpression = ExpressionHelper.ReplaceExpression(expression, parsedValues);
        
        var ncalcExpression = expressionFactory.Create(replacedExpression, _expressionContext);
        return ncalcExpression.Evaluate();
    }

    public ValueTask<object?> EvaluateAsync(string expression, Dictionary<string, object?> parsedValues)
    {
        return new ValueTask<object?>(Evaluate(expression, parsedValues));
    }
}