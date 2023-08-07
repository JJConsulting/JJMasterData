#nullable enable

using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Protheus;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;


namespace JJMasterData.Core.DataManager.Services;

public class ExpressionsService : IExpressionsService
{
    #region "Properties"

    private IHttpContext CurrentContext { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    private ILogger<ExpressionsService> Logger { get; }
    private IEntityRepository EntityRepository { get; }

    #endregion

    #region "Constructors"

    public ExpressionsService(
        IEntityRepository entityRepository,
        IHttpContext httpContext,
        IStringLocalizer<JJMasterDataResources> stringLocalizer,
        ILogger<ExpressionsService> logger)
    {
        EntityRepository = entityRepository;
        CurrentContext = httpContext!;
        StringLocalizer = stringLocalizer;
        Logger = logger;
    }

    #endregion

    public string? ParseExpression(
        string? expression,
        PageState state,
        bool quotationMarks,
        IDictionary<string, dynamic> values,
        IDictionary<string, dynamic>? userValues = null,
        ExpressionManagerInterval? interval = null)
    {
        var formStateData = new FormStateData(values, userValues, state);
        return ParseExpression(expression, formStateData, quotationMarks, interval);
    }

    public string? ParseExpression(
        string? expression,
        FormStateData formStateData,
        bool quotationMarks,
        ExpressionManagerInterval? interval = null)
    {
        if (expression is null)
            return null;

        var parsedExpression = expression
            .Replace("val:", "")
            .Replace("exp:", "")
            .Replace("sql:", "")
            .Replace("protheus:", "")
            .Trim();

        interval ??= new ExpressionManagerInterval('{', '}');

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
                val = $"{CurrentContext.Request["objname"]}";
            }
            else if (CurrentContext.Session?[field] != null)
            {
                val = CurrentContext.Session[field];
            }
            else
            {
                val = "";
            }

            if (val == null) continue;

            if (quotationMarks)
                val = "'" + val + "'";

            if (interval.Begin == '{' && interval.End == '}')
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

    public async Task<string?> GetDefaultValueAsync(ElementField field, FormStateData formStateData)
    {
        return await GetExpressionValueAsync(field.DefaultValue, field, formStateData);
    }

    public async Task<bool> GetBoolValueAsync(string expression, FormStateData formStateData)
    {
        if (string.IsNullOrEmpty(expression))
            throw new ArgumentNullException(nameof(expression));

        bool result;
        if (expression.StartsWith("val:"))
        {
            result = ParseBool(expression);
        }
        else if (expression.StartsWith("exp:"))
        {
            var exp = "";
            try
            {
                exp = ParseExpression(expression, formStateData, true);
                var dt = new DataTable("temp");
                result = (bool)await Task.Run(() => dt.Compute(exp, ""));
                dt.Dispose();
            }
            catch (Exception ex)
            {
                string err = $"Error executing expression {exp}";
                err += " " + ex.Message;
                throw new ArgumentException(err, nameof(expression));
            }
        }
        else if (expression.StartsWith("sql:"))
        {
            var exp = ParseExpression(expression, formStateData, false);
            var obj = await EntityRepository.GetResultAsync(exp);
            result = ParseBool(obj);
        }
        else
        {
            throw new JJMasterDataException($"Expression do not starts with allowed types (val, exp, sql): {expression}");
        }

        return result;
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
        if (string.IsNullOrEmpty(expression))
            return null;

        if (field == null)
            throw new ArgumentNullException(nameof(field), StringLocalizer["FormElementField can not be null"]);

        string? retVal = null;
        try
        {
            if (expression!.StartsWith("val:"))
            {
                if (expression.Contains("{"))
                    retVal = ParseExpression(expression, formStateData, false);
                else
                    retVal = expression.Replace("val:", "").Trim();
            }
            else if (expression.StartsWith("exp:"))
            {
                try
                {
                    var exp = ParseExpression(expression, formStateData, false);
                    if (field.DataType == FieldType.Float)
                        exp = exp?.Replace(".", "").Replace(",", ".");

                    retVal = exp; //When parse is string id
                    var dt = new DataTable();
                    retVal = dt.Compute(exp, "").ToString();
                    dt.Dispose();
                }
                catch (Exception ex)
                {
                    var message = new StringBuilder();
                    message.AppendLine(StringLocalizer["Error executing expression of field {0}.", field.Name]);
                    message.Append(ex.Message);
                    Logger.LogError(ex, message.ToString());
                }
            }
            else if (expression.StartsWith("sql:"))
            {
                var exp = ParseExpression(expression, formStateData, false);
                var obj = await EntityRepository.GetResultAsync(exp);
                if (obj != null)
                    retVal = obj.ToString();
            }
            else if (expression.StartsWith("protheus:"))
            {
                var exp = expression.Replace("\"", "").Replace("'", "").Split(',');
                if (exp.Length < 3)
                    throw new JJMasterDataException(StringLocalizer["Invalid Protheus Request"]);

                var urlProtheus = ParseExpression(exp[0], formStateData, false);
                var functionName = ParseExpression(exp[1], formStateData, false);
                var parms = "";
                if (exp.Length >= 3)
                    parms = ParseExpression(exp[2], formStateData, false);

                retVal = ProtheusManager.CallOrcLib(urlProtheus, functionName, parms);
            }
            else
            {
                var errorMessage = new StringBuilder();
                errorMessage.Append(StringLocalizer["Expression not started with"]);
                errorMessage.Append(" [val, exp, sql or protheus]. ");
                errorMessage.Append(StringLocalizer["Field"]);
                errorMessage.Append(": ");
                errorMessage.Append(field.Name);
                errorMessage.Append(StringLocalizer["Content"]);
                errorMessage.Append(": ");
                errorMessage.Append(expression);
                throw new ArgumentException(errorMessage.ToString());
            }
        }
        catch (ProtheusException ex)
        {
            var errorMessage = new StringBuilder();
            errorMessage.Append(StringLocalizer["Error retrieving expression in Protheus integration."]);
            errorMessage.Append(" ");
            errorMessage.Append(StringLocalizer["Field"]);
            errorMessage.Append(": ");
            errorMessage.AppendLine(field.Name);
            errorMessage.Append(ex.Message);
            var exception = new JJMasterDataException(errorMessage.ToString(), ex);
            Logger.LogError(exception, exception.Message);
            throw exception;
        }
        catch (Exception ex)
        {
            var errorMessage = new StringBuilder();
            errorMessage.AppendLine(StringLocalizer["Error retrieving expression or trigger."]);
            errorMessage.Append(StringLocalizer["Field"]);
            errorMessage.Append(": ");
            errorMessage.AppendLine(field.Name);

            var exception = new JJMasterDataException(errorMessage.ToString(), ex);

            Logger.LogError(exception, exception.Message);

            throw exception;
        }

        return retVal;
    }

    private bool ParseBool(object? value) => StringManager.ParseBool(value);
}