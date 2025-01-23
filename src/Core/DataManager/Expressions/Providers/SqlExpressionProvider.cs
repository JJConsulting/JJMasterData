#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Core.DataManager.Expressions.Abstractions;

namespace JJMasterData.Core.DataManager.Expressions.Providers;

public sealed class SqlExpressionProvider(IEntityRepository entityRepository) : IAsyncExpressionProvider
{
    public const string Prefix = "sql";
    string IExpressionProvider.Prefix => Prefix;
    public string Title => "SQL";
    public Guid? ConnectionId { get; set; }

    public ValueTask<object?> EvaluateAsync(string expression, Dictionary<string,object?> parsedValues)
    {
        var command = ExpressionDataAccessCommandFactory.Create(expression, parsedValues);
        
        return new(entityRepository.GetResultAsync(command,ConnectionId));
    }
}
