#nullable enable
using System;
using System.Collections.Generic;
using System.Data;
using JJMasterData.Commons.Data;

namespace JJMasterData.Core.DataManager.Expressions;

public static class ExpressionDataAccessCommandFactory
{
    public static DataAccessCommand Create(string expression, Dictionary<string, object?> parsedValues)
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
                value = stringValue.Trim(); //this prevents errors when coalescing string to numeric values.
            
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