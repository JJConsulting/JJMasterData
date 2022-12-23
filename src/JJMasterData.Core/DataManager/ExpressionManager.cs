using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Logging;
using JJMasterData.Commons.Protheus;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Http;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.DataManager;

public class ExpressionManager
{
    #region "Properties"
    
    internal IHttpContext CurrentContext { get; }

    internal IEntityRepository EntityRepository { get; }

    public Hashtable UserValues { get; set; }

    #endregion

    #region "Constructors"

    public ExpressionManager(Hashtable userValues, IEntityRepository entityRepository, IHttpContext currentContext)
    {
        UserValues = userValues;
        EntityRepository = entityRepository;
        CurrentContext = currentContext;
    }

    #endregion

    public string ParseExpression(string expression, PageState state, bool quotationMarks, Hashtable formValues, ExpressionManagerInterval interval = null)
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

        List<string> list = StringManager.FindValuesByInterval(expression, interval.Begin, interval.End);

        foreach (string field in list)
        {
            string val = null;
            if (UserValues.Contains(field))
            {
                val = $"{UserValues[field]}";
            }
            else if (formValues != null && formValues.Contains(field))
            {
                var objVal = formValues[field];
                val = objVal != null ? $"{objVal}" : "";
            }
            else if ("pagestate".Equals(field.ToLower()))
            {
                val = $"{state}";
            }
            else if ("objname".Equals(field.ToLower()))
            {
                val = $"{CurrentContext.Request["objname"]}";
            }
            else if (CurrentContext.Session?[field] != null)
            {
                val = $"{CurrentContext.Session[field]}";
            }
            else if (CurrentContext.HasClaimsIdentity())
            {
                val = CurrentContext.GetClaim(field) ?? string.Empty;
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
                parsedExpression = parsedExpression.Replace(string.Format($"{interval.Begin}{{0}}{interval.End}", field), val);
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

    public string GetTriggerValue(FormElementField f, PageState state, Hashtable formValues)
    {
        if (f == null)
            throw new ArgumentNullException(nameof(f), Translate.Key("FormElementField can not be null"));

        return GetValueExpression(f.TriggerExpression, f, state, formValues);
    }

    private string GetValueExpression(string expression, ElementField f, PageState state, Hashtable formValues)
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
                    Log.AddError(ex, message.ToString());
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
            Log.AddError(exception, exception.Message);
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
            
            Log.AddError(exception, exception.Message);

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