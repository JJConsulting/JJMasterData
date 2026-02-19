#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.Core.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCalc;
using NCalc.Factories;

namespace JJMasterData.Core.DataManager.Expressions.Providers;

public sealed class DefaultExpressionProvider(
    IExpressionFactory expressionFactory,
    IServiceProvider serviceProvider,
    IOptions<MasterDataCoreOptions> options,
    ILogger<DefaultExpressionProvider> logger)
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
        var parameters = new Dictionary<string, object?>(parsedValues.Count, StringComparer.InvariantCultureIgnoreCase);
        var preparedExpression = PrepareExpressionWithParameters(expression, parsedValues, parameters);
        
        foreach (var parameter in parameters)
            _expressionContext.StaticParameters[parameter.Key] = parameter.Value;
        
        var ncalcExpression = expressionFactory.Create(preparedExpression, _expressionContext);
        
        logger.LogExpression(preparedExpression);
        
        return ncalcExpression.Evaluate();
    }

    public ValueTask<object?> EvaluateAsync(string expression, Dictionary<string, object?> parsedValues)
    {
        return new ValueTask<object?>(Evaluate(expression, parsedValues));
    }

    private static string PrepareExpressionWithParameters(
        string expression,
        Dictionary<string, object?> parsedValues,
        Dictionary<string, object?> parameters)
    {
        foreach (var kvp in parsedValues)
        {
            var token = $"{ExpressionHelper.Begin}{kvp.Key}{ExpressionHelper.End}";
            var quotedToken = $"'{token}'";

            if (expression.Contains(quotedToken, StringComparison.Ordinal))
            {
                expression = expression.Replace(quotedToken, kvp.Key);
                parameters[kvp.Key] = kvp.Value?.ToString();
            }
            else
            {
                parameters[kvp.Key] = kvp.Value;
            }
        }

        return expression;
    }
}
