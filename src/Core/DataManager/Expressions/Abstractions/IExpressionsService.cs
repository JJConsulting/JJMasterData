#nullable enable

using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager.Services;

namespace JJMasterData.Core.DataManager.Expressions.Abstractions;

public interface IExpressionsService
{
    string? ParseExpression(
        string? expression,
        FormStateData formStateData,
        bool addQuotationMarks = false,
        ExpressionParserInterval? interval = null);
    
    Task<bool> GetBoolValueAsync(string expression, FormStateData formStateData);
    
    Task<string?> GetDefaultValueAsync(ElementField field, FormStateData formStateData);
    
    Task<string?> GetTriggerValueAsync(FormElementField field, FormStateData formStateData);

}