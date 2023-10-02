using System.Threading.Tasks;
using JJMasterData.Core.DataManager.Models;

namespace JJMasterData.Core.DataManager.Expressions.Abstractions;

public interface IExpressionProvider
{
    bool CanHandle(string expressionType);
    Task<object> EvaluateAsync(string expression, FormStateData formStateData);
}