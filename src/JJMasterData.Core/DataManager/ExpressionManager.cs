using System;
using System.Collections;
using System.Data;
using System.Text;
using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Dao.Entity.Abstractions;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Protheus;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.DataManager;

public class ExpressionManager
{
    #region "Properties"

    internal IHttpContext CurrentContext { get; }

    internal IEntityRepository EntityRepository { get; }

    public Hashtable UserValues { get; set; }

    private ILogger<ExpressionManager> Logger { get; }

    #endregion

    #region "Constructors"

    public ExpressionManager(
        Hashtable userValues,
        IEntityRepository entityRepository,
        IHttpContext currentContext,
        ILoggerFactory loggerFactory
    )
    {
        UserValues = userValues;
        Logger = loggerFactory.CreateLogger<ExpressionManager>();
        EntityRepository = entityRepository;
        CurrentContext = currentContext;
    }

    #endregion

    public string ParseExpression(string expression, PageState state, bool quotationMarks, Hashtable formValues,
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
            string value;
            if (UserValues.Contains(field))
            {
                value = $"{UserValues[field]}";
            }
            else if (formValues != null && formValues.Contains(field))
            {
                var objVal = formValues[field];
                value = objVal != null ? $"{objVal}" : "";
            }
            else if ("pagestate".Equals(field.ToLower()))
            {
                value = $"{state}";
            }
            else if ("objname".Equals(field.ToLower()))
            {
                value = $"{CurrentContext.Request["objname"]}";
            }
            else if (CurrentContext.Session?[field] != null)
            {
                value = $"{CurrentContext.Session[field]}";
            }
            else if (CurrentContext.HasClaimsIdentity())
            {
                value = CurrentContext.GetClaim(field) ?? string.Empty;
            }
            else
            {
                value = "";
            }

            if (quotationMarks)
                value = "'" + value + "'";

            if (interval.Begin == '{' && interval.End == '}')
            {
                parsedExpression = parsedExpression.Replace($"{{{field}}}", value);
            }
            else
            {
                parsedExpression =
                    parsedExpression.Replace(string.Format($"{interval.Begin}{{0}}{interval.End}", field), value);
            }
        }

        return parsedExpression;
    }

    public string GetDefaultValue(ElementField f, PageState state, Hashtable formValues)
    {
        if (f == null)
            throw new ArgumentNullException(nameof(f), Translate.Key("ElementField can not be null"));

        return GetValueExpression(f.DefaultValue, f, state, formValues);
    }

    public bool GetBoolValue(string expression, string actionName, PageState state, Hashtable formValues)
    {
        if (string.IsNullOrEmpty(expression))
        {
            string err = $"Invalid expression for {actionName} field";
            throw new ArgumentNullException(nameof(expression), err);
        }

        bool result;
        if (expression.StartsWith("val:"))
        {
            result = ParseBool(expression);
        }
        else if (expression.StartsWith("exp:"))
        {
            string parsedExpression = null;
            try
            {
                parsedExpression = ParseExpression(expression, state, true, formValues);
                var dt = new DataTable("temp");
                result = (bool)dt.Compute(parsedExpression, "");
                dt.Dispose();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error executing expression {parsedExpression} for {actionName}", parsedExpression,
                    actionName);

                throw;
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

    public string GetTriggerValue(FormElementField field, PageState state, Hashtable formValues)
    {
        if (field == null)
            throw new ArgumentNullException(nameof(field), "FormElementField cannot be null");

        return GetValueExpression(field.TriggerExpression, field, state, formValues);
    }

    private string GetValueExpression(string expression, ElementField field, PageState state, Hashtable formValues)
    {
        if (string.IsNullOrEmpty(expression))
            return null;

        if (field == null)
            throw new ArgumentNullException(nameof(field), "FormElementField cannot be null");

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
                    if (field.DataType == FieldType.Float)
                        exp = exp.Replace(".", "").Replace(",", ".");

                    retVal = exp; //When parse is string id
                    var dt = new DataTable();
                    retVal = dt.Compute(exp, "").ToString();
                    dt.Dispose();
                }
                catch (Exception ex)
                {
                    var message = new StringBuilder();
                    message.Append(ex.Message);
                    Logger.LogError(ex, "Error executing expression for field {field}.", field.Name);
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
                errorMessage.Append(field.Name);
                errorMessage.Append(Translate.Key("Content"));
                errorMessage.Append(": ");
                errorMessage.Append(expression);
                throw new ArgumentException(errorMessage.ToString());
            }
        }
        catch (ProtheusException ex)
        {
            var errorMessage = new StringBuilder();

            const string message = "Error retrieving expression in Protheus integration.";

            errorMessage.Append(message);
            errorMessage.Append(' ');
            errorMessage.Append("Field");
            errorMessage.Append(": ");
            errorMessage.AppendLine(field.Name);
            errorMessage.Append(ex.Message);
            var exception = new JJMasterDataException(errorMessage.ToString(), ex);

            Logger.LogError(exception, message);

            throw exception;
        }
        catch (Exception ex)
        {
            var errorMessage = new StringBuilder();

            const string message = "Error while retriving value from expression.";

            errorMessage.AppendLine(message);
            errorMessage.Append("Field");
            errorMessage.Append(": ");
            errorMessage.AppendLine(field.Name);

            var exception = new JJMasterDataException(errorMessage.ToString(), ex);

            Logger.LogError(exception, message);

            throw exception;
        }

        return retVal;
    }


    public static bool ParseBool(object value)
    {
        if (value == null)
            return false;

        string sValue = value.ToString();

        // 1
        // Avoid exceptions
        if (string.IsNullOrWhiteSpace(sValue))
        {
            return false;
        }

        // 2
        // Remove whitespace from string
        sValue = sValue.Replace("val:", "").Trim();

        // 3
        // Lowercase the string
        sValue = sValue.ToLower();

        // 4
        // Check for word true
        return sValue switch
        {
            "true" => true,
            "t" => true,
            "1" => true,
            "yes" => true,
            "y" => true,
            "s" => true,
            _ => false,
        };
    }
}