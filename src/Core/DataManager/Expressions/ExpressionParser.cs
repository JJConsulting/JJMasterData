#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.Logging;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.DataManager.Expressions;

public class ExpressionParser(IHttpContext httpContext, ILogger<ExpressionParser> logger)
{
    private IHttpContext HttpContext { get; } = httpContext;
    private ILogger<ExpressionParser> Logger { get; } = logger;
    private IHttpRequest Request => HttpContext.Request;
    private IHttpSession Session => HttpContext.Session;

    public Dictionary<string, object?> ParseExpression(
        string? expression,
        FormStateData formStateData)
    {
        var result = new Dictionary<string, object?>();

        if (expression is null)
            return result;

        var valueList = StringManager.FindValuesByInterval(expression, ExpressionHelper.Begin, ExpressionHelper.End);

        foreach (var field in valueList)
        {
            var value = GetParsedValue(field, formStateData);
            result[field] = value;
            Logger.LogExpressionParsedValue(field, value);
        }

        return result;
    }
    
    private object? GetParsedValue(string field, FormStateData formStateData)
    {
        var loweredFieldName = field.ToLower();
        object? parsedValue;
            
        var (values, userValues, pageState) = formStateData;
            
        switch (loweredFieldName)
        {
            case "pagestate":
                parsedValue = pageState.GetPageStateName();
                break;
            case "islist":
                parsedValue = pageState is PageState.List ? 1 : 0;
                break;
            case "isview":
                parsedValue = pageState is PageState.View ? 1 : 0;
                break;
            case "isupdate":
                parsedValue = pageState is PageState.Update ? 1 : 0;
                break;
            case "isinsert":
                parsedValue = pageState is PageState.Insert ? 1 : 0;
                break;
            case "isfilter":
                parsedValue = pageState is PageState.Filter ? 1 : 0;
                break;
            case "isimport":
                parsedValue = pageState is PageState.Import ? 1 : 0;
                break;
            case "isdelete":
                parsedValue = pageState is PageState.Delete ? 1 : 0;
                break;
            case "fieldname":
                parsedValue = $"{Request.QueryString["fieldName"]}";
                break;
            case "userid":
                parsedValue = DataHelper.GetCurrentUserId(HttpContext, userValues!);
                break;
            default:
            {
                if(userValues != null && userValues.TryGetValue(field, out var value))
                    parsedValue = value;
                    
                else if (values.TryGetValue(field, out var objValue) && 
                         !string.IsNullOrEmpty(objValue?.ToString()))
                {
                    if (objValue is bool boolValue)
                        parsedValue = boolValue ? "1" : "0";
                    else
                        parsedValue = objValue;
                }
                
                else if (Session.HasSession() && Session[field] != null)
                    parsedValue = Session[field];
                else if (HttpContext.User?.HasClaim(c => c.Type == field) ?? false)
                    parsedValue = HttpContext.User.Claims.First(c => c.Type == field).Value;
                else
                    parsedValue = string.Empty;
                break;
            }
        }

        return parsedValue;
    }
}