#nullable enable
using System;


namespace JJMasterData.Commons.Data.Entity.Models;

public class ConnectionString
{
    public Guid Guid { get; set; }
    public string? Name { get; set; }
    public string Connection { get; set; }
    public DataAccessProvider Provider { get; set; }
    
    public ConnectionString(string connectionString, DataAccessProvider connectionProvider)
    {
        Connection = connectionString;
        Provider = connectionProvider;
    }
}