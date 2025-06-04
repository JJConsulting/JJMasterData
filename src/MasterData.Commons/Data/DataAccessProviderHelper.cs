using System;
using JJMasterData.Commons.Exceptions;

namespace JJMasterData.Commons.Data;

public static class DataAccessProviderHelper
{
    public static string GetAdoNetTypeName(this DataAccessProvider provider)
    {
        return provider switch
        {
            DataAccessProvider.SqlServer => "Microsoft.Data.SqlClient",
            DataAccessProvider.SQLite => "System.Data.SQLite",
            DataAccessProvider.Oracle => "Oracle.ManagedDataAccess.Client",
            DataAccessProvider.OracleNetCore => "Oracle.ManagedDataAccess.Core.Client",
            DataAccessProvider.MySql => "MySql.Data.MySqlClient.MySqlClientFactory",
            DataAccessProvider.PostgreSql => "Npgsql.NpgsqlFactory",
            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null)
        };
    }
    
    public static DataAccessProvider GetDataAccessProviderFromString(string providerType)
    {
        return providerType switch
        {
            "SqlServer" => DataAccessProvider.SqlServer,
            "Microsoft.Data.SqlClient" => DataAccessProvider.SqlServer,
            "SQLite" => DataAccessProvider.SQLite,
            "System.Data.SQLite" => DataAccessProvider.SQLite,
            "Oracle" => DataAccessProvider.Oracle,
            "Oracle.ManagedDataAccess.Client" => DataAccessProvider.Oracle,
            "OracleNetCore" => DataAccessProvider.OracleNetCore,
            "Oracle.ManagedDataAccess.Core.Client" => DataAccessProvider.OracleNetCore,
            "MySql" => DataAccessProvider.MySql,
            "MySql.Data.MySqlClient.MySqlClientFactory" => DataAccessProvider.MySql,
            "PostgreSql" => DataAccessProvider.PostgreSql,
            "Npgsql.NpgsqlFactory" => DataAccessProvider.PostgreSql,
            _ => throw new DataAccessProviderException("Unknown data access provider name.")
        };
    }
}