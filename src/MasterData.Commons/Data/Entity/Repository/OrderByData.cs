#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using JJMasterData.Commons.Data.Entity.Models;

namespace JJMasterData.Commons.Data.Entity.Repository;

public class OrderByData
{
    private readonly Dictionary<string, OrderByDirection> _fields = new();

    public bool Any() => _fields.Count > 0;
    
    public string ToQueryParameter()
    {
        var queryParameter = new StringBuilder();
        
        foreach (var field in _fields)
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
        _fields[fieldName] = direction;
        return this;
    }
    
    public static OrderByData FromString(string? orderByString)
    {
        var orderByData = new OrderByData();
        if (!string.IsNullOrEmpty(orderByString))
            orderByData.Set(orderByString);

        return orderByData;
    }

    public bool Validate(ElementFieldList elementFields)
    {
        if (_fields.Count == 0)
            return true;

        foreach (var field in _fields)
        {
            if (!elementFields.ContainsKey(field.Key))
                return false;
        }

        return true;
    }

    public void Set(string? orderBy)
    {
        _fields.Clear();
        
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

            _fields[fieldName] = direction;
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