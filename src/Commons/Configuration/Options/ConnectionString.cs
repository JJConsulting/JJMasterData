#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;

namespace JJMasterData.Commons.Configuration.Options;

public class ConnectionString()
{
    [SetsRequiredMembers]
    public ConnectionString(string connectionString, string connectionProvider) : this()
    {
        Connection = connectionString;
        ConnectionProvider = connectionProvider;
    }

    public Guid Guid { get; init; } = Guid.NewGuid();
    public required string? Name { get; init; }
    public required string Connection { get; init; }
    public required string ConnectionProvider { get; init; }
}