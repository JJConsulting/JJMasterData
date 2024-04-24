#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;


namespace JJMasterData.Commons.Data.Entity.Models;

public class ConnectionString
{
    public ConnectionString()
    {
        
    }
    
    [SetsRequiredMembers]
    public ConnectionString(string connectionString, DataAccessProvider connectionProvider)
    {
        Connection = connectionString;
        Provider = connectionProvider;
    }

    public Guid Guid { get; set; }
    public required string? Name { get; set; }
    public required string Connection { get; set; }
    public DataAccessProvider Provider { get; set; }
}