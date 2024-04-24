using System;
using JJMasterData.Commons.Configuration.Options;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Data.Entity.Providers;

public class SqlServerInfo(IOptionsSnapshot<MasterDataCommonsOptions> commonsOptions)
{
    private static int _majorVersion;
    private static int _compatibilityLevel;

    private DataAccess GetDataAccess(Guid? connectionId)
    {
        var connection = commonsOptions.Value.GetConnectionString(connectionId);
        return new DataAccess(connection.Connection, connection.Provider);
    }
    
    public int GetMajorVersion(Guid? connectionId)
    {
        if (_majorVersion > 0)
            return _majorVersion;

        try
        {
            var ret = GetDataAccess(connectionId).GetResult("SELECT SERVERPROPERTY('productversion')");
            var version = ret!.ToString().Split('.')[0];
            _majorVersion = int.Parse(version);
        }
        catch
        {
            _majorVersion = 1;
        }

        return _majorVersion;
    }

    public int GetCompatibilityLevel(Guid? connectionId)
    {
        if (_compatibilityLevel > 0)
            return _compatibilityLevel;

        try
        {
            var ret = GetDataAccess(connectionId).GetResult("select compatibility_level from sys.databases where name = DB_NAME()");
            _compatibilityLevel = int.Parse(ret!.ToString());
        }
        catch
        {
            _compatibilityLevel = 100;
        }

        return _compatibilityLevel;
    }
}


