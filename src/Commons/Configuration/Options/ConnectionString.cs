#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;

namespace JJMasterData.Commons.Configuration.Options;

public class ConnectionString
{
    public ConnectionString()
    {
        
    }
    
    [SetsRequiredMembers]
    public ConnectionString(string connectionString, string connectionProvider)
    {
        Connection = connectionString;
        ConnectionProvider = connectionProvider;
    }

    public Guid Guid { get; set; }
    public required string? Name { get; set; }
    public required string Connection { get; set; }
    public required string ConnectionProvider { get; set; }
}