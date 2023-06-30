#nullable enable
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Protheus;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.DataManager.Services;

public class ExpressionManagerInterval
{
    public char Begin { get; set; }
    public char End { get; set; }

    public ExpressionManagerInterval(char begin, char end)
    {
        Begin = begin;
        End = end;
    }
}

public class ExpressionsService : IExpressionsService
{
    #region "Properties"

    private IHttpContext CurrentContext { get; }
    private ILogger<ExpressionsService> Logger { get; }
    private IEntityRepository EntityRepository { get; }

    #endregion

    #region "Constructors"

    public ExpressionsService(
        IEntityRepository entityRepository,
        IHttpContext httpContext,
        ILogger<ExpressionsService> logger)
    {
        EntityRepository = entityRepository;
        CurrentContext = httpContext!;
        Logger = logger;
    }

    #endregion

    public string ParseExpression(string expression,
        PageState state,
        bool quotationMarks,
        IDictionary<string, dynamic>? values,
        IDictionary<string, dynamic?>? userValues = null,
        ExpressionManagerInterval interval = null)
    {
        if (expression == null)
            return null;

        string parsedExpression = expression
            .Replace("val:", "")
            .Replace("exp:", "")
            .Replace("sql:", "")
            .Replace("protheus:", "")
            .Trim();

        interval ??= new ExpressionManagerInterval('{', '}');

        var list = StringManager.FindValuesByInterval(expression, interval.Begin, interval.End);

        foreach (string field in list)
        {
            string? val = null;
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


    public string GetDefaultValue(ElementField f, PageState state, IDictionary<string, dynamic?> formValues,
        IDictionary<string, dynamic?>? userValues = null)
    {
        if (f == null)
            throw new ArgumentNullException(nameof(f), Translate.Key("ElementField can not be null"));

        return GetValueExpression(f.DefaultValue, f, state, formValues);
    }

    public bool GetBoolValue(string expression, string actionName, PageState state,
        IDictionary<string, dynamic?> formValues, IDictionary<string, dynamic?>? userValues = null)
    {
        if (string.IsNullOrEmpty(expression))
        {
            string err = Translate.Key("Invalid expression for {0} field", actionName);
            throw new ArgumentNullException(nameof(expression), err);
        }

        bool result;
        if (expression.StartsWith("val:"))
        {
            result = ParseBool(expression);
        }
        else if (expression.StartsWith("exp:"))
        {
            string exp = "";
            try
            {
                exp = ParseExpression(expression, state, true, formValues);
                var dt = new DataTable("temp");
                result = (bool)dt.Compute(exp, "");
                dt.Dispose();
            }
            catch (Exception ex)
            {
                string err = Translate.Key("Error executing expression {0} for {1} field.", exp, actionName);
                err += " " + ex.Message;
                throw new ArgumentException(err, nameof(expression));
            }
        }
        else if (expression.StartsWith("sql:"))
        {
            string exp = ParseExpression(expression, state, false, formValues);
            object obj = EntityRepository.GetResult(exp);
            result = ParseBool(obj);
        }
        else
        {
            string err = Translate.Key("Invalid expression for {0} field", actionName);
            throw new ArgumentException(err, nameof(expression));
        }

        return result;
    }

    public async Task<bool> GetBoolValueAsync(string expression, string actionName, PageState state,
        IDictionary<string, dynamic?>? formValues = null,
        IDictionary<string, dynamic?>? userValues = null)
    {
        if (string.IsNullOrEmpty(expression))
        {
            string err = Translate.Key("Invalid expression for {0} field", actionName);
            throw new ArgumentNullException(nameof(expression), err);
        }

        bool result;
        if (expression.StartsWith("val:"))
        {
            result = ParseBool(expression);
        }
        else if (expression.StartsWith("exp:"))
        {
            string exp = "";
            try
            {
                exp = ParseExpression(expression, state, true, formValues);
                var dt = new DataTable("temp");
                result = (bool)await Task.Run(() => dt.Compute(exp, ""));
                dt.Dispose();
            }
            catch (Exception ex)
            {
                string err = Translate.Key("Error executing expression {0} for {1} field.", exp, actionName);
                err += " " + ex.Message;
                throw new ArgumentException(err, nameof(expression));
            }
        }
        else if (expression.StartsWith("sql:"))
        {
            string exp = ParseExpression(expression, state, false, formValues);
            object obj = await EntityRepository.GetResultAsync(exp);
            result = ParseBool(obj);
        }
        else
        {
            string err = Translate.Key("Invalid expression for {0} field", actionName);
            throw new ArgumentException(err, nameof(expression));
        }

        return result;
    }

    public string GetTriggerValue(FormElementField f, PageState state, IDictionary<string, dynamic?> formValues,
        IDictionary<string, dynamic?>? userValues = null)
    {
        if (f == null)
            throw new ArgumentNullException(nameof(f), Translate.Key("FormElementField can not be null"));

        return GetValueExpression(f.TriggerExpression, f, state, formValues);
    }

    public string GetValueExpression(string expression, ElementField f, PageState state,
        IDictionary<string, dynamic?> formValues, IDictionary<string, dynamic?>? userValues = null)
    {
        if (string.IsNullOrEmpty(expression))
            return null;

        if (f == null)
            throw new ArgumentNullException(nameof(f), Translate.Key("FormElementField can not be null"));

        string retVal = null;
        try
        {
            if (expression.StartsWith("val:"))
            {
                if (expression.Contains("{"))
                    retVal = ParseExpression(expression, state, false, formValues);
                else
                    retVal = expression.Replace("val:", "").Trim();
            }
            else if (expression.StartsWith("exp:"))
            {
                try
                {
                    string exp = ParseExpression(expression, state, false, formValues);
                    if (f.DataType == FieldType.Float)
                        exp = exp.Replace(".", "").Replace(",", ".");

                    retVal = exp; //When parse is string id
                    var dt = new DataTable();
                    retVal = dt.Compute(exp, "").ToString();
                    dt.Dispose();
                }
                catch (Exception ex)
                {
                    var message = new StringBuilder();
                    message.AppendLine(Translate.Key("Error executing expression of field {0}.", f.Name));
                    message.Append(ex.Message);
                    Logger.LogError(ex, message.ToString());
                }
            }
            else if (expression.StartsWith("sql:"))
            {
                string exp = ParseExpression(expression, state, false, formValues);
                object obj = EntityRepository.GetResult(exp);
                if (obj != null)
                    retVal = obj.ToString();
            }
            else if (expression.StartsWith("protheus:"))
            {
                string[] exp = expression.Replace("\"", "").Replace("'", "").Split(',');
                if (exp.Length < 3)
                    throw new JJMasterDataException(Translate.Key("Invalid Protheus Request"));

                string urlProtheus = ParseExpression(exp[0], state, false, formValues);
                string functionName = ParseExpression(exp[1], state, false, formValues);
                string parms = "";
                if (exp.Length >= 3)
                    parms = ParseExpression(exp[2], state, false, formValues);

                retVal = ProtheusManager.CallOrcLib(urlProtheus, functionName, parms);
            }
            else
            {
                var errorMessage = new StringBuilder();
                errorMessage.Append(Translate.Key("Expression not started with"));
                errorMessage.Append(" [val, exp, sql or protheus]. ");
                errorMessage.Append(Translate.Key("Field"));
                errorMessage.Append(": ");
                errorMessage.Append(f.Name);
                errorMessage.Append(Translate.Key("Content"));
                errorMessage.Append(": ");
                errorMessage.Append(expression);
                throw new ArgumentException(errorMessage.ToString());
            }
        }
        catch (ProtheusException ex)
        {
            var errorMessage = new StringBuilder();
            errorMessage.Append(Translate.Key("Error retrieving expression in Protheus integration."));
            errorMessage.Append(" ");
            errorMessage.Append(Translate.Key("Field"));
            errorMessage.Append(": ");
            errorMessage.AppendLine(f.Name);
            errorMessage.Append(ex.Message);
            var exception = new JJMasterDataException(errorMessage.ToString(), ex);
            Logger.LogError(exception, exception.Message);
            throw exception;
        }
        catch (Exception ex)
        {
            var errorMessage = new StringBuilder();
            errorMessage.AppendLine(Translate.Key("Error retrieving expression or trigger."));
            errorMessage.Append(Translate.Key("Field"));
            errorMessage.Append(": ");
            errorMessage.AppendLine(f.Name);

            var exception = new JJMasterDataException(errorMessage.ToString(), ex);

            Logger.LogError(exception, exception.Message);

            throw exception;
        }

        return retVal;
    }


    public bool ParseBool(object? value) => StringManager.ParseBool(value);
}