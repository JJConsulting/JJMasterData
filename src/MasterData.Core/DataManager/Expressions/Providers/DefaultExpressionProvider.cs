using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.Core.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCalc;
using NCalc.Factories;
using NCalc.Handlers;

namespace JJMasterData.Core.DataManager.Expressions.Providers;

public sealed class DefaultExpressionProvider(
    IExpressionFactory expressionFactory,
    IServiceProvider serviceProvider,
    IOptions<MasterDataCoreOptions> options,
    ILogger<DefaultExpressionProvider> logger)
    : ISyncExpressionProvider, IAsyncExpressionProvider
{
    public string Prefix => "exp";
    public string Title => "Expression";

    public Guid? ConnectionId { get; set; }
    
    public object? Evaluate(string expression, Dictionary<string, object?> parsedValues)
    {
        var parameters = new Dictionary<string, object?>(parsedValues.Count, StringComparer.InvariantCultureIgnoreCase);
        var preparedExpression = PrepareExpressionWithParameters(expression, parsedValues, parameters);

        var expressionContext = new ExpressionContext(options.Value.ExpressionContext)
        {
            Parameters = new Dictionary<string, object?>(parameters, StringComparer.InvariantCultureIgnoreCase)
            {
                ["ServiceProvider"] = serviceProvider
            }
        };
        
        var ncalcExpression = expressionFactory.Create(preparedExpression, options.Value.ExpressionConfiguration, expressionContext);
        
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
            var value = kvp.Value is DBNull ? null : kvp.Value;

            if (expression.Contains(quotedToken, StringComparison.InvariantCultureIgnoreCase))
            {
                expression = expression.Replace(quotedToken, kvp.Key);
                parameters[kvp.Key] = value?.ToString();
            }
            else
            {
                parameters[kvp.Key] = value;
            }
        }

        return expression;
    }
}
