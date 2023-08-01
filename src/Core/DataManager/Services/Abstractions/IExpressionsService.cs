#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Core.DataManager.Services.Abstractions;

public interface IExpressionsService
{
    string? ParseExpression(
        string? expression,
        PageState state,
        bool quotationMarks,
        IDictionary<string, dynamic?>? values,
        IDictionary<string,dynamic?>? userValues = null,
        ExpressionManagerInterval? interval = null);

    Task<string?> GetDefaultValueAsync(ElementField field, PageState state, IDictionary<string,dynamic?> formValues, IDictionary<string,dynamic?>? userValues = null);
    Task<bool> GetBoolValueAsync(string expression, PageState state, IDictionary<string,dynamic?>? formValues = null, IDictionary<string,dynamic?>? userValues = null);
    Task<string?> GetTriggerValueAsync(FormElementField field, PageState state, IDictionary<string,dynamic?> formValues, IDictionary<string,dynamic?>? userValues = null);
    bool ParseBool(object value);
}