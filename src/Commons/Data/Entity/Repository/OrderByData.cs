#nullable enable
using System;
using System.Collections.Generic;
using JJMasterData.Commons.Util;

namespace JJMasterData.Commons.Data.Entity;

public class OrderByData
{
    private Dictionary<string,OrderByDirection> Fields { get; }
    
    // ReSharper disable once ConvertConstructorToMemberInitializers
    public OrderByData()
    {
        Fields = new Dictionary<string, OrderByDirection>();
    }

    public string? ToQueryParameter()
    {
        string? queryParameter = null;
        
        foreach (var field in Fields)
        {
            if (queryParameter != null)
            {
                queryParameter += ",";
            }

            queryParameter += field.Key + " " + field.Value.ToString().ToUpper();

        }

        return queryParameter;
    }
    
    public OrderByData AddOrReplace(string fieldName, OrderByDirection direction)
    {
        Fields[fieldName] = direction;
        return this;
    }
    
    public static OrderByData FromString(string orderByString)
    {
        var orderByData = new OrderByData();
        
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
            
            var directionStr = OrderByDirection.Asc.ToString(); 
            
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