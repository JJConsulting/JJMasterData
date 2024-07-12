#nullable enable

using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using Microsoft.Extensions.Options;
using NCalc.Factories;

namespace JJMasterData.Core.DataManager.Expressions.Providers;

public sealed class DefaultExpressionProvider(IExpressionFactory expressionFactory, IOptions<MasterDataCoreOptions> options) : ISyncExpressionProvider, IAsyncExpressionProvider
{
    public string Prefix => "exp";
    public string Title => "Expression";
    
    public object? Evaluate(string expression, Dictionary<string, object?> parsedValues)
    {
        var replacedExpression = ExpressionHelper.ReplaceExpression(expression, parsedValues);
        var ncalcExpression = expressionFactory.Create(replacedExpression, options.Value.ExpressionsContext);
        return ncalcExpression.Evaluate();
    }

    public ValueTask<object?> EvaluateAsync(string expression, Dictionary<string, object?> parsedValues) =>
        new(Evaluate(expression, parsedValues));
}