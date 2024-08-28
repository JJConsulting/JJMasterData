using System;

namespace JJMasterData.Commons.Data.Entity.Repository;

public enum OrderByDirection
{
    Asc,
    Desc
}

public static class OrderByDirectionExtensions
{
    public static string GetSqlString(this OrderByDirection direction)
    {
        return direction switch
        {
            OrderByDirection.Asc => "ASC",
            OrderByDirection.Desc => "DESC",
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
    }
}