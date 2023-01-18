using System.Collections.Generic;
using JJMasterData.Commons.Data;

namespace JJMasterData.Commons.Extensions;

public static class DataAccessExtensions
{
    public static T GetModel<T>(this DataAccess dataAccess, string sql)
    {
        return dataAccess.GetFields(sql).ToModel<T>();
    }

    public static T GetModel<T>(this DataAccess dataAccess, DataAccessCommand cmd)
    {
        return dataAccess.GetFields(cmd).ToModel<T>();
    }

    public static List<T> GetModelList<T>(this DataAccess dataAccess, string sql)
    {
        return dataAccess.GetDataTable(sql).ToModelList<T>();
    }

    public static List<T> GetModelList<T>(this DataAccess dataAccess, DataAccessCommand cmd)
    {
        return dataAccess.GetDataTable(cmd).ToModelList<T>();
    }
}
