#nullable enable
using System;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.DataManager.Services;


public class ExpressionParser : IExpressionParser
{
    private IHttpRequest Request { get; }
    private IHttpSession Session { get; }
    
    public ExpressionParser(IHttpRequest request, IHttpSession session)
    {
        Request = request;
        Session = session;
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
        var values = formStateData.FormValues;

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

            if (addQuotationMarks)
                parsedValue = $"'{parsedValue}'";
            
            parsedExpression = parsedExpression.Replace($"{interval.Begin}{field}{interval.End}", parsedValue);
        }

        return parsedExpression;
    }
}

