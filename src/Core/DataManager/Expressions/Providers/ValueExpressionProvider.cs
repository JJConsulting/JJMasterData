#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Core.DataManager.Expressions.Abstractions;

namespace JJMasterData.Core.DataManager.Expressions.Providers;

public sealed class ValueExpressionProvider : IAsyncExpressionProvider, ISyncExpressionProvider
{
    public const string Prefix = "val";
    
    public Guid? ConnectionId { get; set; }
    
    string IExpressionProvider.Prefix => Prefix;
    public string Title => "Value";
    public object Evaluate(string expression, Dictionary<string, object?> parsedValues)
    {
        if (expression.Contains(ExpressionHelper.Begin.ToString()))
            return ExpressionHelper.ReplaceExpression(expression, parsedValues).Trim();

        return expression.Trim();
    }

    public ValueTask<object?> EvaluateAsync(string expression, Dictionary<string,object?> parsedValues)
        => new(Evaluate(expression, parsedValues));
}