using JJMasterData.Core.DataManager.Models;

namespace JJMasterData.Core.DataManager.Expressions.Abstractions;

public interface IBooleanExpressionProvider : IExpressionProvider
{
    bool Evaluate(string expression, FormStateData formStateData);
}