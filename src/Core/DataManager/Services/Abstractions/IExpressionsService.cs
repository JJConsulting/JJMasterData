#nullable enable

using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.DataDictionary;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JJMasterData.Core.DataManager.Services.Abstractions;

public interface IExpressionsService
{
    string? ParseExpression(
        string? expression,
        PageState state,
        bool quotationMarks,
        IDictionary<string, dynamic> values,
        IDictionary<string,dynamic>? userValues = null,
        ExpressionManagerInterval? interval = null);

    string? ParseExpression(
        string? expression,
        FormStateData formStateData,
        bool quotationMarks,
        ExpressionManagerInterval? interval = null);
    
    Task<bool> GetBoolValueAsync(string expression, FormStateData formStateData);
    
    Task<string?> GetDefaultValueAsync(ElementField field, FormStateData formStateData);
    
    Task<string?> GetTriggerValueAsync(FormElementField field, FormStateData formStateData);

}