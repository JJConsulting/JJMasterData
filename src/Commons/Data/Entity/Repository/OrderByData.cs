#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using JJMasterData.Commons.Util;

namespace JJMasterData.Commons.Data.Entity.Repository;

public class OrderByData
{
    private Dictionary<string,OrderByDirection> Fields { get; } = new();

    public bool Any() => Fields.Count > 0;
    
    public string ToQueryParameter()
    {
        var queryParameter = new StringBuilder();
        
        foreach (var field in Fields)
        {
            if (queryParameter.Length > 0)
            {
                queryParameter.Append(',');
            }

            queryParameter.Append($"{field.Key} {field.Value.GetSqlString()}");
        }

        return queryParameter.ToString();
    }
    
    public OrderByData AddOrReplace(string fieldName, OrderByDirection direction)
    {
        Fields[fieldName] = direction;
        return this;
    }
    
    public static OrderByData FromString(string? orderByString)
    {
        var orderByData = new OrderByData();
        if (!string.IsNullOrEmpty(orderByString))
            orderByData.Set(orderByString);

        return orderByData;
    }

    public void Set(string? orderBy)
    {
        Fields.Clear();
        
        if (orderBy == null || string.IsNullOrEmpty(orderBy))
        {
            return;
        }

        var fields = orderBy.Split(',');

        foreach (var field in fields)
        {
            var fieldParts = field.Split(' ');
            
            var directionStr = nameof(OrderByDirection.Asc);
            
            if(fieldParts.Length == 2)
                directionStr = fieldParts[1].ToUpperInvariant();

            var fieldName = fieldParts[0];

            var direction = ParseDirection(directionStr);

            Fields[fieldName] = direction;
        }
    }

    private static OrderByDirection ParseDirection(string directionStr)
    {
        return directionStr.ToLowerInvariant() switch
        {
            "asc" => OrderByDirection.Asc,
            "desc" => OrderByDirection.Desc,
            _ => throw new ArgumentException("Invalid OrderBy direction.")
        };
    }
}