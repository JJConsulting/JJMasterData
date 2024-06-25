#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.Core.DataManager.Expressions.Providers;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Logging;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.DataManager.Expressions;

public class ExpressionsService(
    IEnumerable<IExpressionProvider> expressionProviders,
    ExpressionParser expressionParser,
    IEncryptionService encryptionService,
    ILogger<ExpressionsService> logger)
{
    private record struct Expression(string Prefix, string Content);

    private string? _valueExpressionPrefix;

    private string ValueExpressionPrefix => _valueExpressionPrefix ??= 
        ExpressionProviders.First(p => p is ValueExpressionProvider).Prefix;

    private IEnumerable<IExpressionProvider> ExpressionProviders { get; } = expressionProviders;
    private ExpressionParser ExpressionParser { get; } = expressionParser;
    private IEncryptionService EncryptionService { get; } = encryptionService;
    private ILogger<ExpressionsService> Logger { get; } = logger;
    
    public Dictionary<string, object?> ParseExpression(string expression, FormStateData formStateData)
    {
        return ExpressionParser.ParseExpression(expression, formStateData);
    }
    
    public string? ReplaceExpressionWithParsedValues(
        string? expression,
        FormStateData formStateData,
        bool encryptValues = false
        )
    {
        var parsedValues = ExpressionParser.ParseExpression(expression, formStateData);

        if (encryptValues)
            EncryptValues(parsedValues);
        
        if (expression != null)
            return ExpressionHelper.ReplaceExpression(expression, parsedValues);

        return null;
    }

    private void EncryptValues(Dictionary<string, object?> parsedValues)
    {
        foreach(var kvp in parsedValues)
        {
            var value = parsedValues[kvp.Key];
            if(value is not null)
                parsedValues[kvp.Key] = EncryptionService.EncryptStringWithUrlEscape(value.ToString()!);
        }
    }

    public bool GetBoolValue(string? expression, FormStateData formStateData)
    {
        return ParseBool(GetExpressionValue(expression, formStateData));
    }

    public object? GetExpressionValue(string? expression, FormStateData formStateData)
    {
        var extractedExpression = GetExpressionFromString(expression);
        var (expressionType, expressionValue) = extractedExpression;

        if (ExpressionProviders.FirstOrDefault(p => p.Prefix == expressionType && p is ISyncExpressionProvider) is
            not ISyncExpressionProvider provider)
            throw new JJMasterDataException($"Expression type not supported: {expressionType}.");

        object? result;

        try
        {
            Logger.LogExpression(expression);

            var parsedValues = ExpressionParser.ParseExpression(expression, formStateData);

            result = provider.Evaluate(expressionValue, parsedValues);
        }
        catch (Exception ex)
        {
            var exception = new ExpressionException("Unhandled exception at a expression provider.", ex);

            Logger.LogExpressionError(exception, provider.Prefix, expression);

            throw exception;
        }

        return result;
    }

    public ValueTask<object?> GetTriggerValueAsync(FormElementFieldSelector fieldSelector, FormStateData formStateData)
    {
        var field = fieldSelector.Field;
        return GetExpressionValueAsync(fieldSelector,field.TriggerExpression, formStateData);
    }
    
    public ValueTask<object?> GetDefaultValueAsync(FormElementFieldSelector fieldSelector, FormStateData formStateData)
    {
        var field = fieldSelector.Field;
        return GetExpressionValueAsync(fieldSelector,field.DefaultValue, formStateData);
    }
    
    private async ValueTask<object?> GetExpressionValueAsync(
        FormElementFieldSelector fieldSelector,
        string? expression,
        FormStateData formStateData)
    {
        var extractedExpression = GetExpressionFromString(expression);
        var (expressionType, expressionValue) = extractedExpression;

        if (ExpressionProviders.FirstOrDefault(p => p.Prefix == expressionType && p is IAsyncExpressionProvider) is not
            IAsyncExpressionProvider provider)
        {
            throw new JJMasterDataException($"Expression type not supported: {expressionType}");
        }

        var field = fieldSelector.Field;
        
        try
        {
            var parsedValues = ExpressionParser.ParseExpression(expression, formStateData);

            if(provider is SqlExpressionProvider sqlProvider) 
                sqlProvider.ConnectionId = fieldSelector.FormElement.ConnectionId;
            
            var result = await provider.EvaluateAsync(expressionValue, parsedValues);
            if (result is string stringResult)
            {
                return field.DataType switch
                {
                    FieldType.Int when int.TryParse(stringResult.Trim(),
                        out var intResult) => intResult,
                    FieldType.Float when double.TryParse(stringResult.Trim(),
                        out var doubleResult) => doubleResult,
                    _ => result
                };
            }

            return result;
        }
        catch (Exception ex)
        {
            var exception =
                new ExpressionException($"Unhandled exception at a expression provider.\nField: {field.Name}", ex);

            Logger.LogExpressionErrorWithField(exception, provider.Prefix, expression, field.Name);

            throw exception;
        }
    }

    private Expression GetExpressionFromString(string? expression)
    {
        var splitExpression = expression?.Split([':'], 2) ;

        if (splitExpression?.Length < 2)
            return new Expression(ValueExpressionPrefix, expression ?? string.Empty);
        
        var prefix = splitExpression?[0];

        if (!ExpressionProviders.GetProvidersPrefixes().Contains(prefix) || splitExpression is null)
            return new Expression(ValueExpressionPrefix, expression ?? string.Empty);
        
        return new Expression(splitExpression[0], splitExpression[1]);
    }


    private static bool ParseBool(object? value) => StringManager.ParseBool(value);
}