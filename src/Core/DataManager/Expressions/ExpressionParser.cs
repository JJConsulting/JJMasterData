#nullable enable
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.DataManager.Expressions;


public class ExpressionParser
{
    private IHttpContext HttpContext { get; }
    private IHttpRequest Request => HttpContext.Request;
    private IHttpSession Session => HttpContext.Session;
    
    public ExpressionParser(IHttpContext httpContext)
    {
        HttpContext = httpContext;
    }
    
    public string? ParseExpression(
        string? expression,
        FormStateData formStateData,
        bool addQuotationMarks = false,
        ExpressionParserInterval? interval = null)
    {
        if (expression is null)
            return null;

        var parsedExpression = expression;
        
        if (expression.Contains(":"))
        {
            parsedExpression = expression.Split(':')[1];
        }

        interval ??= new ExpressionParserInterval();

        var valueList = StringManager.FindValuesByInterval(expression, interval.Begin, interval.End);
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
            else if (values != null && values.TryGetValue(field, out var objVal))
            {
                parsedValue = objVal != null ? $"{objVal}" : "";
            }
            else if ("objname".Equals(field.ToLower()))
            {
                parsedValue = $"{Request["componentName"]}";
            }
            else if ("componentName".Equals(field.ToLower()))
            {
                parsedValue = $"{Request["componentName"]}";
            }
            else if ("userid".Equals(field.ToLower()))
            {
                parsedValue = DataHelper.GetCurrentUserId(HttpContext,userValues!);
            }
            else if (Session?[field] != null)
            {
                parsedValue = Session[field];
            }
            else
            {
                parsedValue = string.Empty;
            }

            if (parsedValue == null) 
                continue;

            if (addQuotationMarks && !double.TryParse(parsedValue, out _))
                parsedValue = $"'{parsedValue}'";
            
            parsedExpression = parsedExpression.Replace($"{interval.Begin}{field}{interval.End}", parsedValue);
        }

        return parsedExpression;
    }
}

