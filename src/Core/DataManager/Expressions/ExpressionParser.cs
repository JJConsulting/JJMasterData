#nullable enable
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
        bool addQuotationMarks,
        ExpressionParserInterval? interval = null)
    {
        if (expression is null)
            return null;

        var parsedExpression = expression
            .Replace("val:", "")
            .Replace("exp:", "")
            .Replace("sql:", "")
            .Replace("protheus:", "")
            .Trim();

        interval ??= new ExpressionParserInterval('{', '}');

        var list = StringManager.FindValuesByInterval(expression, interval.Begin, interval.End);
        var userValues = formStateData.UserValues;
        var state = formStateData.PageState;
        var values = formStateData.FormValues;

        foreach (var field in list)
        {
            string? val;
            if (userValues != null && userValues.TryGetValue(field, out var value))
            {
                val = $"{value}";
            }
            else if ("pagestate".Equals(field.ToLower()))
            {
                val = $"{state}";
            }
            else if (values != null && values.TryGetValue(field, out var objVal))
            {
                val = objVal != null ? $"{objVal}" : "";
            }
            else if ("objname".Equals(field.ToLower()))
            {
                val = $"{Request["componentName"]}";
            }
            else if ("componentName".Equals(field.ToLower()))
            {
                val = $"{Request["componentName"]}";
            }
            else if (Session?[field] != null)
            {
                val = Session[field];
            }
            else
            {
                val = "";
            }

            if (val == null) continue;

            if (addQuotationMarks)
                val = $"'{val}'";

            if (interval is { Begin: '{', End: '}' })
            {
                parsedExpression = parsedExpression.Replace($"{{{field}}}", val);
            }
            else
            {
                parsedExpression =
                    parsedExpression.Replace(string.Format($"{interval.Begin}{{0}}{interval.End}", field), val);
            }
        }

        return parsedExpression;
    }
}

