using System.Threading.Tasks;
using JJMasterData.Core.DataManager.Models;

namespace JJMasterData.Core.DataManager.Expressions.Abstractions;

public interface IExpressionProvider
{
    string Prefix { get; }
    Task<object> EvaluateAsync(string expression, FormStateData formStateData);
}