using System.Threading.Tasks;

namespace JJMasterData.Core.DataManager.Services;

public interface IExpressionProvider
{
    bool CanHandle(string expressionType);
    Task<object> EvaluateAsync(string expression, FormStateData formStateData);
}