namespace JJMasterData.Commons.Data.Entity.Providers;

public class SqlServerInfo
{
    private readonly DataAccess dataAccess;
    private static int majorVersion;
    private static int compatibilityLevel;

    public SqlServerInfo(DataAccess dataAccess)
    {
        this.dataAccess = dataAccess;
    }

    public int GetMajorVersion()
    {
        if (majorVersion > 0)
            return majorVersion;

        try
        {
            var ret = dataAccess.GetResult("SELECT SERVERPROPERTY('productversion')");
            var version = ret.ToString().Split('.')[0];
            majorVersion = int.Parse(version);
        }
        catch
        {
            majorVersion = 1;
        }

        return majorVersion;
    }

    public int GetCompatibilityLevel()
    {
        if (compatibilityLevel > 0)
            return compatibilityLevel;

        try
        {
            var ret = dataAccess.GetResult("select compatibility_level from sys.databases where name = DB_NAME()");
            var version = ret.ToString();
            compatibilityLevel = int.Parse(version);
        }
        catch
        {
            compatibilityLevel = 100;
        }

        return compatibilityLevel;
    }
}


