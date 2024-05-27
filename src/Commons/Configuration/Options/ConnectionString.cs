#nullable enable
using System;

namespace JJMasterData.Commons.Configuration.Options;

public class ConnectionString()
{
    public ConnectionString(string connectionString, string connectionProvider) : this()
    {
        Connection = connectionString;
        ConnectionProvider = connectionProvider;
    }

    public Guid Guid { get; init; } = Guid.NewGuid();
    public string? Name { get; init; }
    public string Connection { get; init; } = null!;
    public string ConnectionProvider { get; init; } = null!;
}