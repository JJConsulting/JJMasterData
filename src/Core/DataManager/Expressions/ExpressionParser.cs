#nullable enable
using System;
using System.Collections.Generic;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.DataManager.Expressions;

public class ExpressionParser
{
    private IHttpContext HttpContext { get; }
    private ILogger<ExpressionParser> Logger { get; }
    private IHttpRequest Request => HttpContext.Request;
    private IHttpSession Session => HttpContext.Session;
    
    public ExpressionParser(IHttpContext httpContext, ILogger<ExpressionParser> logger)
    {
        HttpContext = httpContext;
        Logger = logger;
    }
    
    public IDictionary<string,object?> ParseExpression(
        string? expression,
        FormStateData formStateData)
    {

        var result = new Dictionary<string, object?>(StringComparer.InvariantCultureIgnoreCase);
        
        if (expression is null)
            return result;
        
        var valueList = StringManager.FindValuesByInterval(expression, ExpressionHelper.Begin, ExpressionHelper.End);
        var userValues = formStateData.UserValues;
        var values = formStateData.Values;
        
        result.Add("PageState", formStateData.PageState);
        
        if (Request.QueryString.TryGetValue("fieldName", out var fieldName))
            result.Add("FieldName", fieldName);
    
        result.Add("UserId", DataHelper.GetCurrentUserId(HttpContext,userValues!));
        
        foreach (var field in valueList)
        {
            string? parsedValue;
            if (userValues != null && userValues.TryGetValue(field, out var value))
            {
                parsedValue = $"{value}";
            }
            else if (values != null && values.TryGetValue(field, out var objVal) && !string.IsNullOrEmpty(objVal?.ToString()))
            {
                parsedValue = $"{objVal}";
            }
            else if (Session?[field] != null)
            {
                parsedValue = Session[field];
            }
            else
            {
                parsedValue = string.Empty;
            }
            
            result[field] = parsedValue;
            
            Logger.LogDebug("Added parsed value to {Field}: {ParsedValue}", field, parsedValue);
        }
        
        return result;
    }
}

