#nullable enable
using System.Collections.Generic;

namespace JJMasterData.Core.DataManager.Expressions.Abstractions;

public interface ISyncExpressionProvider : IExpressionProvider
{
    object? Evaluate(string expression, Dictionary<string, object> parsedValues);
}