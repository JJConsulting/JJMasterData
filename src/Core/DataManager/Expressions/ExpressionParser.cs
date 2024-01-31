#nullable enable
using System.Collections.Generic;
using System.Linq;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models;
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
            object? parsedValue;
            if (userValues != null && userValues.TryGetValue(field, out var value))
            {
                parsedValue = value;
            }
            else if ("pagestate".Equals(field.ToLower()))
            {
                parsedValue = $"{state}";
            }
            else if ("islist".Equals(field.ToLower()))
            {
                parsedValue = state == PageState.List ? "1" : "0";
            }
            else if ("isupdate".Equals(field.ToLower()))
            {
                parsedValue = state == PageState.Update ? "1" : "0";
            }
            else if ("isinsert".Equals(field.ToLower()))
            {
                parsedValue = state == PageState.Insert ? "1" : "0";
            }
            else if (values != null && values.TryGetValue(field, out var objVal) && !string.IsNullOrEmpty(objVal?.ToString()))
            {
                if(objVal is bool boolValue)
                {
                    parsedValue = boolValue ? "1" : "0";
                }
                else
                {
                    parsedValue = objVal;
                }
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
            else if (HttpContext.User?.HasClaim(c => c.Type == field) ?? false)
            {
                parsedValue = HttpContext.User.Claims.First(c => c.Type == field).Value;
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

