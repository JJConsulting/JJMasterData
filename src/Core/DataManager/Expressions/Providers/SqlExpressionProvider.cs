#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Core.DataManager.Expressions.Abstractions;

namespace JJMasterData.Core.DataManager.Expressions.Providers;

internal class SqlExpressionProvider : IAsyncExpressionProvider
{
    private readonly IEntityRepository _entityRepository;

    public SqlExpressionProvider(IEntityRepository entityRepository)
    {
        _entityRepository = entityRepository;
    }

    public string Prefix => "sql";
    public string Title => "T-SQL";
    
    public async Task<object?> EvaluateAsync(string expression, IDictionary<string,object?> parsedValues)
    {
        var command = GetParsedDataAccessCommand(expression, parsedValues);

        var result = await _entityRepository.GetResultAsync(command);
        
        return result?.ToString() ?? null;
    }

    internal static DataAccessCommand GetParsedDataAccessCommand(string expression, IDictionary<string, object?> parsedValues)
    {
        var command = new DataAccessCommand();

        foreach (var kvp in parsedValues)
        {
            var parameterName = $"@{kvp.Key}";

            expression = expression.Replace($"'{ExpressionHelper.Begin}{kvp.Key}{ExpressionHelper.End}'", parameterName);
            expression = expression.Replace($"{ExpressionHelper.Begin}{kvp.Key}{ExpressionHelper.End}", parameterName);

            var value = kvp.Value;

            command.AddParameter(parameterName, value, GetDbTypeFromObject(value));
        }

        command.Sql = expression;
        return command;
    }

    private static DbType GetDbTypeFromObject(object? value)
    {
        return value switch
        {
            int => DbType.Int32,
            float or decimal or double => DbType.Decimal,
            string => DbType.String,
            DateTime => DbType.DateTime,
            bool => DbType.Boolean,
            _ => DbType.String
        };
    }
}
