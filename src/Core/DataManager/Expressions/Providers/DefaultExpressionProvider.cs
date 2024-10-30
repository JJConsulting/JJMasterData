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
    IServiceProvider serviceProvider,
    IOptions<MasterDataCoreOptions> options) : ISyncExpressionProvider, IAsyncExpressionProvider
{
    public string Prefix => "exp";
    public string Title => "Expression";

    public Guid? ConnectionId { get; set; }
    
    public object? Evaluate(string expression, Dictionary<string, object?> parsedValues)
    {
        var replacedExpression = ExpressionHelper.ReplaceExpression(expression, parsedValues);

        options.Value.ExpressionContext.StaticParameters["ServiceProvider"] = serviceProvider;
        
        var ncalcExpression = expressionFactory.Create(replacedExpression, options.Value.ExpressionContext);
        return ncalcExpression.Evaluate();
    }

    public ValueTask<object?> EvaluateAsync(string expression, Dictionary<string, object?> parsedValues)
    {
        return new ValueTask<object?>(Evaluate(expression,parsedValues));
    }
}