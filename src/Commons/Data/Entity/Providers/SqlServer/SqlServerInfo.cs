﻿namespace JJMasterData.Commons.Data.Entity.Providers;

public class SqlServerInfo(DataAccess dataAccess)
{
    private static int _majorVersion;
    private static int _compatibilityLevel;

    public int GetMajorVersion()
    {
        if (_majorVersion > 0)
            return _majorVersion;

        try
        {
            var ret = dataAccess.GetResult("SELECT SERVERPROPERTY('productversion')");
            var version = ret!.ToString().Split('.')[0];
            _majorVersion = int.Parse(version);
        }
        catch
        {
            _majorVersion = 1;
        }

        return _majorVersion;
    }

    public int GetCompatibilityLevel()
    {
        if (_compatibilityLevel > 0)
            return _compatibilityLevel;

        try
        {
            var ret = dataAccess.GetResult("select compatibility_level from sys.databases where name = DB_NAME()");
            _compatibilityLevel = int.Parse(ret!.ToString());
        }
        catch
        {
            _compatibilityLevel = 100;
        }

        return _compatibilityLevel;
    }
}


