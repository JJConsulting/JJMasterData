using System.ComponentModel;

namespace JJMasterData.Commons.Data;

public enum DataAccessProvider
{
    [Description("Microsoft.Data.SqlClient")]
    SqlServer,
    [Description("System.Data.SQLite")]
    SqLite,
    [Description("Oracle.ManagedDataAccess.Client")]
    Oracle,
    [Description("Oracle.ManagedDataAccess.Core.Client")]
    OracleNetCore,
    [Description("MySql.Data.MySqlClient.MySqlClientFactory")]
    MySql,
    [Description("Npgsql.NpgsqlFactory")]
    PostgreSql
}
