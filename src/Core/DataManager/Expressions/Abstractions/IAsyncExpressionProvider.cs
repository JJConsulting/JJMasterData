#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;


namespace JJMasterData.Core.DataManager.Expressions.Abstractions;

public interface IAsyncExpressionProvider : IExpressionProvider
{
    ValueTask<object?> EvaluateAsync(string expression, Dictionary<string,object?> parsedValues);
}