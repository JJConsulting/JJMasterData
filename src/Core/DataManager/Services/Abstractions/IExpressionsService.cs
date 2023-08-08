#nullable enable

using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.DataDictionary;
using System.Threading.Tasks;

namespace JJMasterData.Core.DataManager.Services.Abstractions;

public interface IExpressionsService
{
    string? ParseExpression(
        string? expression,
        FormStateData formStateData,
        bool quotationMarks,
        ExpressionManagerInterval? interval = null);
    
    Task<bool> GetBoolValueAsync(string expression, FormStateData formStateData);
    
    Task<string?> GetDefaultValueAsync(ElementField field, FormStateData formStateData);
    
    Task<string?> GetTriggerValueAsync(FormElementField field, FormStateData formStateData);

}