#nullable enable
using System;
using JJMasterData.Commons.Util;

namespace JJMasterData.Commons.Data.Entity;

public record OrderByData(
    string FieldName,
    OrderByDirection Direction = OrderByDirection.Asc
)
{
    public override string ToString()
    {
        return FieldName + " " + Direction.ToString().ToUpper();
    }
    
    public static OrderByData? FromString(string? orderBy)
    {
        if (string.IsNullOrEmpty(orderBy))
        {
            return null;
        }

        string[] parts = orderBy.Split(' ');
        if (parts.Length != 2)
        {
            throw new ArgumentException("Invalid format for orderBy. The correct format is 'FieldName Direction'.");
        }

        string fieldName = parts[0];
        string directionStr = parts[1].ToUpper();

        if (!Enum.TryParse(directionStr.ToLower().FirstCharToUpper(), out OrderByDirection direction) ||
            !Enum.IsDefined(typeof(OrderByDirection), direction))
        {
            throw new ArgumentException("Invalid value for Direction in orderBy.");
        }

        return new OrderByData(fieldName, direction);
    }
}