#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Core.DataManager.Expressions.Abstractions;

namespace JJMasterData.Core.DataManager.Expressions.Providers;

public class SqlExpressionProvider(IEntityRepository entityRepository) : IAsyncExpressionProvider
{
    public string Prefix => "sql";
    public string Title => "SQL";
    
    public async Task<object?> EvaluateAsync(string expression, Dictionary<string,object?> parsedValues)
    {
        var command = GetParsedDataAccessCommand(expression, parsedValues);

        var result = await entityRepository.GetResultAsync(command);
            
        return result;
    }

    internal static DataAccessCommand GetParsedDataAccessCommand(string expression, IDictionary<string, object?> parsedValues)
    {
        var command = new DataAccessCommand();

        foreach (var keyValuePair in parsedValues)
        {
            var parameterName = $"@{keyValuePair.Key}";

            expression = expression.Replace($"'{ExpressionHelper.Begin}{keyValuePair.Key}{ExpressionHelper.End}'", $" {parameterName} ");
            expression = expression.Replace($"{ExpressionHelper.Begin}{keyValuePair.Key}{ExpressionHelper.End}", $" {parameterName} ");

            var value = keyValuePair.Value;

            if (value is string stringValue)
            {
                value = stringValue.Trim(); //this prevents erros when coalescing string to numeric values.
            }
            
            command.AddParameter(parameterName, value, GetDbTypeFromObject(keyValuePair.Value));
        }

        command.Sql = expression;
        return command;
    }

    private static DbType GetDbTypeFromObject(object? value)
    {
        return value switch
        {
            int => DbType.Int32,
            double => DbType.Double,
            decimal => DbType.Decimal,
            float => DbType.Single,
            string => DbType.String,
            DateTime => DbType.DateTime,
            bool => DbType.Boolean,
            _ => DbType.String
        };
    }
}
