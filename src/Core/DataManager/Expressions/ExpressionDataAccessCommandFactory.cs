#nullable enable
using System;
using System.Collections.Generic;
using System.Data;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Util;

namespace JJMasterData.Core.DataManager.Expressions;

public static class ExpressionDataAccessCommandFactory
{
    public static DataAccessCommand Create(string expression, Dictionary<string, object?> parsedValues)
    {
        var command = new DataAccessCommand();

        var variables = StringManager.FindValuesByInterval(expression, ExpressionHelper.Begin, ExpressionHelper.End);

        foreach (var variable in variables)
        {
            if(!parsedValues.ContainsKey(variable) && !variable.Contains("\n"))
                parsedValues.Add(variable, null);
        }

        foreach (var keyValuePair in parsedValues)
        {
            DbType dbType;
            var parameterName = $"@{keyValuePair.Key}";
            var oldExpression = expression;

            expression = expression.Replace($"'{ExpressionHelper.Begin}{keyValuePair.Key}{ExpressionHelper.End}'",
                $" {parameterName} ");

            object? value;
            
            if (oldExpression != expression)
            {
                dbType = DbType.AnsiString;
                value = keyValuePair.Value?.ToString();
            }
            else
            {
                expression = expression.Replace($"{ExpressionHelper.Begin}{keyValuePair.Key}{ExpressionHelper.End}",
                    $" {parameterName} ");

                dbType = GetDbTypeFromObject(keyValuePair.Value);
                value = keyValuePair.Value;
            }
            
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
            string => DbType.AnsiString,
            Guid => DbType.Guid,
            DateTime => DbType.DateTime,
            bool => DbType.Boolean,
            _ => DbType.AnsiString
        };
    }
}