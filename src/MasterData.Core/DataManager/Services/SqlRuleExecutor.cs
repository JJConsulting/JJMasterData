#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services.Abstractions;

namespace JJMasterData.Core.DataManager.Services;

public class SqlRuleExecutor(IEntityRepository entityRepository) : IRuleExecutor
{
    public RuleLanguage Language => RuleLanguage.Sql;

    public async Task<Dictionary<string, string>> ExecuteAsync(
        FormElement formElement,
        FormElementRule rule,
        Dictionary<string, object?> values)
    {
        var command = ExpressionDataAccessCommandFactory.Create(
            rule.Script,
            values);
        
        var dataSet = await entityRepository.GetDataSetAsync(command, formElement.ConnectionId);

        var errors = new Dictionary<string, string>();

        if (dataSet.Tables.Count == 0)
            return errors;

        foreach (DataTable dataTable in dataSet.Tables)
        {
            if (dataTable.Rows.Count == 0)
                continue;

            var index = 0;

            foreach (DataRow row in dataTable.Rows)
            {
                index++;

                var (key, message) = GetError(row, index);

                if (!string.IsNullOrWhiteSpace(key) &&
                    !string.IsNullOrWhiteSpace(message) &&
                    !errors.ContainsKey(key))
                {
                    errors[key] = message;
                }
            }
        }

        return errors;
    }

    private static (string Key, string Message) GetError(
        DataRow row,
        int index)
    {
        var columns = row.Table.Columns;

        // Just one column?
        if (columns.Count == 1)
        {
            var message = row[0]?.ToString() ?? string.Empty;
            return (index.ToString(), message);
        }

        var first = row[0]?.ToString();
        var second = row[1]?.ToString() ?? string.Empty;

        var key = string.IsNullOrWhiteSpace(first)
            ? index.ToString()
            : first!;

        return (key, second);
    }
}
