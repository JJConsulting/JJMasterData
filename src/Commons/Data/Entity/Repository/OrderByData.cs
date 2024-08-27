#nullable enable
using System;
using System.Collections.Generic;
using JJMasterData.Commons.Util;

namespace JJMasterData.Commons.Data.Entity.Repository;

public class OrderByData
{
    private Dictionary<string,OrderByDirection> Fields { get; } = new();

    public bool Any() => Fields.Count > 0;
    
    public string? ToQueryParameter()
    {
        string? queryParameter = null;
        
        foreach (var field in Fields)
        {
            if (queryParameter != null)
            {
                queryParameter += ",";
            }

            queryParameter += $"{field.Key} {field.Value.ToString().ToUpper()}";

        }

        return queryParameter;
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
            string[] fieldParts = field.Split(' ');
            
            var directionStr = nameof(OrderByDirection.Asc);
            
            if(fieldParts.Length == 2)
                directionStr = fieldParts[1].ToUpper();

            var fieldName = fieldParts[0];
            
            if (!Enum.TryParse(directionStr.ToLower().FirstCharToUpper(), out OrderByDirection direction) ||
                !Enum.IsDefined(typeof(OrderByDirection), direction))
            {
                throw new ArgumentException("Invalid value for Direction in orderBy.");
            }

            Fields[fieldName] = direction;
        }
    }
}