#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.Core.DataManager.Models;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.DataManager.Expressions;

public class ExpressionsService(
    IEnumerable<IExpressionProvider> expressionProviders,
    ExpressionParser expressionParser,
    ILogger<ExpressionsService> logger)
{
    private IEnumerable<IExpressionProvider> ExpressionProviders { get; } = expressionProviders;
    private ExpressionParser ExpressionParser { get; } = expressionParser;
    private ILogger<ExpressionsService> Logger { get; } = logger;

    public Task<object?> GetDefaultValueAsync(ElementField field, FormStateData formStateData)
    {
        return GetExpressionValueAsync(field.DefaultValue, field, formStateData);
    }

    public string? ReplaceExpressionWithParsedValues(
        string? expression, 
        FormStateData formStateData)
    {
        var parsedValues = ExpressionParser.ParseExpression(expression, formStateData);

        if (expression != null) 
            return ExpressionHelper.ReplaceExpression(expression, parsedValues);

        return null;
    }

    public bool GetBoolValue(string expression, FormStateData formStateData)
    {
        if (string.IsNullOrEmpty(expression))
            throw new ArgumentNullException(nameof(expression));

        var splittedExpression = expression.Split([':'], 2);
        var expressionType = splittedExpression[0];
        if (ExpressionProviders.FirstOrDefault(p => p.Prefix == expressionType && p is IBooleanExpressionProvider) is not IBooleanExpressionProvider provider)
            throw new JJMasterDataException($"Expression type not supported: {expressionType}.");

        object? result;
        
        try
        {
            Logger.LogDebug("Executing expression: {Expression}", expression);

            var parsedValues = ExpressionParser.ParseExpression(expression, formStateData);
            var parsedExpression = splittedExpression[1];
            
            result = provider.Evaluate(parsedExpression, parsedValues);
        }

        catch (Exception ex)
        {
            var exception = new ExpressionException("Unhandled exception at a expression provider.",ex);

            Logger.LogError(exception,"Error retrieving expression at {Provider} provider. Expression: {Expression}",provider.Prefix, expression);

            throw exception;
        }
        
        return ParseBool(result);
    }

    public Task<object?> GetTriggerValueAsync(FormElementField field, FormStateData formStateData)
    {
        return GetExpressionValueAsync(field.TriggerExpression, field, formStateData);
    }

    private async Task<object?> GetExpressionValueAsync(
        string? expression,
        ElementField field,
        FormStateData formStateData)
    {
        if (expression is null || string.IsNullOrEmpty(expression))
            return null;

        if (field == null)
            throw new ArgumentNullException(nameof(field));

        var splittedExpression = expression.Split([':'], 2);
        var expressionType =splittedExpression[0];
        if (ExpressionProviders.FirstOrDefault(p => p.Prefix == expressionType && p is IAsyncExpressionProvider) is not IAsyncExpressionProvider provider)
        {
            throw new JJMasterDataException($"Expression type not supported: {expressionType}");
        }
        try
        {
            var parsedValues = ExpressionParser.ParseExpression(expression, formStateData);
            var parsedExpression = splittedExpression[1];
            var result = await provider.EvaluateAsync(parsedExpression, parsedValues);

            if (result is string stringResult)
            {
                if (field.DataType is FieldType.Int && int.TryParse(stringResult.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var intResult))
                {
                    return intResult;
                }
                if (field.DataType is FieldType.Float && float.TryParse(stringResult.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var floatResult))
                {
                    return floatResult;
                }
            }
            
            return result;
        }

        catch (Exception ex)
        {
            var exception = new ExpressionException("Unhandled exception at a expression provider.",ex);

            Logger.LogError(exception,"Error retrieving expression at {Provider} provider\nExpression: {Expression}\nField: {FieldName}",provider,expression, field.Name);

            throw exception;
        }
    }

    private static bool ParseBool(object? value) => StringManager.ParseBool(value);
}