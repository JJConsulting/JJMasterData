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
    public Guid? ConnectionId { get; set; }

    public async Task<object?> EvaluateAsync(string expression, Dictionary<string,object?> parsedValues)
    {
        var command = GetParsedDataAccessCommand(expression, parsedValues);

        var result = await entityRepository.GetResultAsync(command,ConnectionId);
            
        return result;
    }

    internal static DataAccessCommand GetParsedDataAccessCommand(string expression, Dictionary<string, object?> parsedValues)
    {
        var command = new DataAccessCommand();

        foreach (var keyValuePair in parsedValues)
        {
            DbType dbType;
            var parameterName = $"@{keyValuePair.Key}";
            var oldExpression = expression;
            
            expression = expression.Replace($"'{ExpressionHelper.Begin}{keyValuePair.Key}{ExpressionHelper.End}'", $" {parameterName} ");
            
            if (oldExpression != expression)
            {
                dbType = DbType.String;
            }
            else
            {
                expression = expression.Replace($"{ExpressionHelper.Begin}{keyValuePair.Key}{ExpressionHelper.End}", $" {parameterName} ");
                
                dbType = GetDbTypeFromObject(keyValuePair.Value);
            }
            var value = keyValuePair.Value;

            if (value is string stringValue)
                value = stringValue.Trim(); //this prevents erros when coalescing string to numeric values.
            
            command.AddParameter(parameterName, value, dbType);
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
            float => DbType.Double,
            string => DbType.String,
            DateTime => DbType.DateTime,
            bool => DbType.Boolean,
            _ => DbType.String
        };
    }
}
