#nullable enable
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

using JJMasterData.Core.DataManager.Expressions.Abstractions;

namespace JJMasterData.Core.DataManager.Expressions.Providers;

public class DefaultExpressionProvider : ISyncExpressionProvider, IAsyncExpressionProvider
{
    public string Prefix => "exp";
    public string Title => "Expression";
    
    private static readonly DataTable _expressionsDataTable = new();
    
    public object Evaluate(string expression, Dictionary<string, object> parsedValues)
    {
        var replacedExpression = ExpressionHelper.ReplaceExpression(expression, parsedValues);
        var result = _expressionsDataTable.Compute(replacedExpression, string.Empty);
        return result!;
    }

    public Task<object?> EvaluateAsync(string expression, Dictionary<string, object> parsedValues) 
        => Task.FromResult<object?>(Evaluate(expression,parsedValues));
}