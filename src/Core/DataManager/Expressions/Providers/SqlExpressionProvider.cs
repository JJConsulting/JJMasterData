#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Core.DataManager.Expressions.Abstractions;

namespace JJMasterData.Core.DataManager.Expressions.Providers;

public sealed class SqlExpressionProvider(IEntityRepository entityRepository) : IAsyncExpressionProvider
{
    public string Prefix => "sql";
    public string Title => "SQL";
    public Guid? ConnectionId { get; set; }

    public async ValueTask<object?> EvaluateAsync(string expression, Dictionary<string,object?> parsedValues)
    {
        var command = ExpressionDataAccessCommandFactory.Create(expression, parsedValues);

        var result = await entityRepository.GetResultAsync(command,ConnectionId);
            
        return result;
    }
}
