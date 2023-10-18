#nullable enable
using System.Threading.Tasks;
using JJMasterData.Core.DataManager.Models;

namespace JJMasterData.Core.DataManager.Expressions.Abstractions;

public interface IAsyncExpressionProvider : IExpressionProvider
{
    Task<object?> EvaluateAsync(string expression, FormStateData formStateData);
}