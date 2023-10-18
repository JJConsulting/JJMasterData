#nullable enable
using System;
using System.Collections.Generic;
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

public class ExpressionsService 
{
    #region "Properties"

    private IEnumerable<IExpressionProvider> ExpressionProviders { get; }
    private ExpressionParser ExpressionParser { get; }
    private ILogger<ExpressionsService> Logger { get; }

    #endregion

    #region "Constructors"

    public ExpressionsService(
        IEnumerable<IExpressionProvider> expressionProviders,
        ExpressionParser expressionParser,
        ILogger<ExpressionsService> logger)
    {
        ExpressionProviders = expressionProviders;
        ExpressionParser = expressionParser;
        Logger = logger;
    }

    #endregion
    
    public async Task<string?> GetDefaultValueAsync(ElementField field, FormStateData formStateData)
    {
        return await GetExpressionValueAsync(field.DefaultValue, field, formStateData);
    }

    public string? ParseExpression(string? expression, FormStateData formStateData, bool addQuotationMarks = false,
        ExpressionParserInterval? interval = null)
    {
        return ExpressionParser.ParseExpression(expression, formStateData, addQuotationMarks, interval);
    }

    public async Task<bool> GetBoolValueAsync(string expression, FormStateData formStateData)
    {
        if (string.IsNullOrEmpty(expression))
            throw new ArgumentNullException(nameof(expression));

        var expressionType = expression.Split(':')[0];
        var provider = ExpressionProviders.FirstOrDefault(p => p.Prefix == expressionType);
        if (provider == null)
        {
            throw new JJMasterDataException($"Expression type not supported: {expressionType}.");
        }

        object? result;
        
        try
        {
            Logger.LogDebug("Executing expression: {Expression}", expression);
            result = await provider.EvaluateAsync(expression, formStateData);
        }

        catch (Exception ex)
        {
            var exception = new ExpressionException("Unhandled exception at a expression provider.",ex);

            Logger.LogError(exception,"Error retrieving expression at {Provider} provider. Expression: {Expression}",provider.Prefix, expression);

            throw exception;
        }
        
        return ParseBool(result);
    }

    public async Task<string?> GetTriggerValueAsync(FormElementField field, FormStateData formStateData)
    {
        return await GetExpressionValueAsync(field.TriggerExpression, field, formStateData);
    }

    private async Task<string?> GetExpressionValueAsync(
        string? expression,
        ElementField field,
        FormStateData formStateData)
    {
        if (expression is null || string.IsNullOrEmpty(expression))
            return null;

        if (field == null)
            throw new ArgumentNullException(nameof(field));

        var expressionType = expression.Split(':')[0];
        var provider = ExpressionProviders.FirstOrDefault(p => p.Prefix == expressionType);
        if (provider == null)
        {
            throw new JJMasterDataException($"Expression type not supported: {expressionType}");
        }
        try
        {
            var result = await provider.EvaluateAsync(expression, formStateData);
            return result?.ToString();
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