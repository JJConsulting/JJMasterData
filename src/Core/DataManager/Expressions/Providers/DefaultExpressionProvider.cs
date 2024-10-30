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

public sealed class DefaultExpressionProvider : ISyncExpressionProvider, IAsyncExpressionProvider
{
    private readonly IExpressionFactory _expressionFactory;
    private readonly ExpressionContext _expressionContext;

    public DefaultExpressionProvider(
        IExpressionFactory expressionFactory,
        IServiceProvider serviceProvider,
        IOptions<MasterDataCoreOptions> options)
    {
        _expressionFactory = expressionFactory;
        _expressionContext = options.Value.ExpressionContext;
        _expressionContext.StaticParameters["ServiceProvider"] = serviceProvider;
    }

    public string Prefix => "exp";
    public string Title => "Expression";

    public Guid? ConnectionId { get; set; }
    
    public object? Evaluate(string expression, Dictionary<string, object?> parsedValues)
    {
        var replacedExpression = ExpressionHelper.ReplaceExpression(expression, parsedValues);
        
        var ncalcExpression = _expressionFactory.Create(replacedExpression, _expressionContext);
        return ncalcExpression.Evaluate();
    }

    public ValueTask<object?> EvaluateAsync(string expression, Dictionary<string, object?> parsedValues)
    {
        return new ValueTask<object?>(Evaluate(expression,parsedValues));
    }
}