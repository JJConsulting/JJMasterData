#nullable enable
using System.Collections.Generic;
using JJMasterData.Core.DataManager.Models;

namespace JJMasterData.Core.DataManager.Expressions.Abstractions;

public interface IBooleanExpressionProvider : IExpressionProvider
{
    bool Evaluate(string expression, IDictionary<string,object?> parsedValues);
}