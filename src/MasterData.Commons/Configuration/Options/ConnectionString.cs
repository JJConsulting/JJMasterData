#nullable enable
using System;
using System.ComponentModel.DataAnnotations;

namespace JJMasterData.Commons.Configuration.Options;

public class ConnectionString()
{
    public ConnectionString(string connectionString, string connectionProvider) : this()
    {
        Connection = connectionString;
        ConnectionProvider = connectionProvider;
    }

    public Guid Guid { get; init; } = Guid.NewGuid();
    
    [Display(Name = "Name")]
    public string Name { get; init; } = null!;
    
    [Display(Name = "Connection")]
    public string Connection { get; init; } = null!;
    
    [Display(Name = "Connection Provider")]
    public string ConnectionProvider { get; set; } = "Microsoft.Data.SqlClient";

    [Display(Name = "Roles")]
    public string[] Roles { get; set; } = [];
}