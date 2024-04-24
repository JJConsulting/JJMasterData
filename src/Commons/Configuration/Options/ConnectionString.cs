#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;

namespace JJMasterData.Commons.Configuration.Options;

public class ConnectionString
{
    public ConnectionString()
    {
        Guid = Guid.NewGuid();
    }
    
    [SetsRequiredMembers]
    public ConnectionString(string connectionString, string connectionProvider)
    {
        Connection = connectionString;
        ConnectionProvider = connectionProvider;
    }

    public Guid Guid { get; init; }
    public required string? Name { get; init; }
    public required string Connection { get; init; }
    public required string ConnectionProvider { get; init; }
}