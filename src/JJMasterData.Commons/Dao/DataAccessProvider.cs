using System;
using System.Data.Common;
using System.Data.SqlClient;
using JJMasterData.Commons.Util;

namespace JJMasterData.Commons.Dao;

public static class DataAccessProvider
{
    public const string Oracle = "System.Data.OracleClient";
    public const string MSSQL = "System.Data.SqlClient";
    public const string SQLite = "System.Data.SQLite";
    public const string IBMDB2 = "IBMDADB2";
    public const string Postgre = "POSTGRE SQL";
    public const string MySQL = "MYSQL";
    public const string Informix = "Informix";
    public const string Sybase = "Sybase";
    
    public enum DataAccessProviderTypes
    {
        SqlServer,
        SqLite,
        MySql,
        PostgreSql,

#if NETFULL
    OleDb,
    SqlServerCompact
#endif
    }

    public static DbProviderFactory GetDbProviderFactory(string dbProviderFactoryTypename, string assemblyName)
    {
        var instance = ReflectionUtils.GetStaticProperty(dbProviderFactoryTypename, "Instance");
        if (instance == null)
        {
            var a = ReflectionUtils.LoadAssembly(assemblyName);
            if (a != null)
                instance = ReflectionUtils.GetStaticProperty(dbProviderFactoryTypename, "Instance");
        }

        if (instance == null)
            throw new InvalidOperationException(string.Format("ERROR", dbProviderFactoryTypename));

        return instance as DbProviderFactory;
    }

    public static DbProviderFactory GetDbProviderFactory(DataAccessProviderTypes type)
    {
        switch (type)
        {
            case DataAccessProviderTypes.SqlServer:
                return SqlClientFactory.Instance; // this library has a ref to SqlClient so this works
            case DataAccessProviderTypes.SqLite:
#if NETFULL
        return GetDbProviderFactory("System.Data.SQLite.SQLiteFactory", "System.Data.SQLite");
#else
                return GetDbProviderFactory("Microsoft.Data.Sqlite.SqliteFactory", "Microsoft.Data.Sqlite");
#endif
            case DataAccessProviderTypes.MySql:
                return GetDbProviderFactory("MySql.Data.MySqlClient.MySqlClientFactory", "MySql.Data");
            case DataAccessProviderTypes.PostgreSql:
                return GetDbProviderFactory("Npgsql.NpgsqlFactory", "Npgsql");
            default:
#if NETFULL
    case DataAccessProviderTypes.OleDb:
        return System.Data.OleDb.OleDbFactory.Instance;
    case DataAccessProviderTypes.SqlServerCompact:
        return DbProviderFactories.GetFactory("System.Data.SqlServerCe.4.0");                
#endif

                throw new NotSupportedException($"Not supported {type}");
        }
    }
    
    public static DbProviderFactory GetDbProviderFactory(string providerName)
    {
#if NETFULL
    return DbProviderFactories.GetFactory(providerName);
#else
        providerName = providerName.ToLower();

        switch (providerName)
        {
            case "system.data.sqlclient":
                return GetDbProviderFactory(DataAccessProviderTypes.SqlServer);
            case "system.data.sqlite":
            case "microsoft.data.sqlite":
                return GetDbProviderFactory(DataAccessProviderTypes.SqLite);
            case "mysql.data.mysqlclient":
            case "mysql.data":
                return GetDbProviderFactory(DataAccessProviderTypes.MySql);
            case "npgsql":
                return GetDbProviderFactory(DataAccessProviderTypes.PostgreSql);
            default:
                throw new NotSupportedException($"Not supported {providerName}");
#endif
        }
    }
}