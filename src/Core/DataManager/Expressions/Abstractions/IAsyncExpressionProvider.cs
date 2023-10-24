#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Core.DataManager.Models;

namespace JJMasterData.Core.DataManager.Expressions.Abstractions;

public interface IAsyncExpressionProvider : IExpressionProvider
{
    Task<object?> EvaluateAsync(string expression, IDictionary<string,object?> parsedValues);
}