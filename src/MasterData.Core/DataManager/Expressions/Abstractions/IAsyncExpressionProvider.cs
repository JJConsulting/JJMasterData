#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace JJMasterData.Core.DataManager.Expressions.Abstractions;

public interface IAsyncExpressionProvider : IExpressionProvider
{
    Guid? ConnectionId { get; set; }
    ValueTask<object?> EvaluateAsync(string expression, Dictionary<string,object?> parsedValues);
}