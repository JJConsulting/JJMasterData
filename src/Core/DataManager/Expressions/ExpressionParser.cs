#nullable enable
using System.Collections.Generic;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.DataManager.Expressions;

public class ExpressionParser(IHttpContext httpContext, ILogger<ExpressionParser> logger)
{
    private IHttpContext HttpContext { get; } = httpContext;
    private ILogger<ExpressionParser> Logger { get; } = logger;
    private IHttpRequest Request => HttpContext.Request;
    private IHttpSession Session => HttpContext.Session;

    public IDictionary<string,object?> ParseExpression(
        string? expression,
        FormStateData formStateData)
    {

        var result = new Dictionary<string, object?>();
        
        if (expression is null)
            return result;
        
        var valueList = StringManager.FindValuesByInterval(expression, ExpressionHelper.Begin, ExpressionHelper.End);
        var userValues = formStateData.UserValues;
        var state = formStateData.PageState;
        var values = formStateData.Values;

        foreach (var field in valueList)
        {
            string? parsedValue;
            if (userValues != null && userValues.TryGetValue(field, out var value))
            {
                parsedValue = $"{value}";
            }
            else if ("pagestate".Equals(field.ToLower()))
            {
                parsedValue = $"{state}";
            }
            else if (values != null && values.TryGetValue(field, out var objVal) && !string.IsNullOrEmpty(objVal?.ToString()))
            {
                parsedValue = $"{objVal}";
            }
            else if ("fieldName".Equals(field.ToLower()))
            {
                parsedValue = $"{Request.QueryString["fieldName"]}";
            }
            else if ("userid".Equals(field.ToLower()))
            {
                parsedValue = DataHelper.GetCurrentUserId(HttpContext,userValues!);
            }
            else if (Session.HasSession() && Session[field] != null)
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

