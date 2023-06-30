using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Core.DataManager.Services.Abstractions;

public interface IExpressionsService
{
    string ParseExpression(string expression,
        PageState state,
        bool quotationMarks,
        IDictionary<string,dynamic>? values,
        IDictionary<string,dynamic?>? userValues = null,
        ExpressionManagerInterval interval = null);

    string? GetDefaultValue(ElementField f, PageState state, IDictionary<string,dynamic?> formValues, IDictionary<string,dynamic?>? userValues = null);
    bool GetBoolValue(string expression, string actionName, PageState state, IDictionary<string,dynamic?> formValues, IDictionary<string,dynamic?>? userValues = null);
    Task<bool> GetBoolValueAsync(string expression, string actionName, PageState state, IDictionary<string,dynamic?>? formValues = null, IDictionary<string,dynamic?>? userValues = null);
    string? GetTriggerValue(FormElementField f, PageState state, IDictionary<string,dynamic?> formValues, IDictionary<string,dynamic?>? userValues = null);
    string? GetValueExpression(string expression, ElementField f, PageState state, IDictionary<string,dynamic?> formValues, IDictionary<string,dynamic?>? userValues = null);
    bool ParseBool(object value);
}