using System.ComponentModel.DataAnnotations;
using System.Data.Common;

namespace JJMasterData.Web.Areas.MasterData.Models;

public class ConnectionString
{
    [Required] public string? Server { get; init; }

    public bool IntegratedSecurity { get; init; }

    [Required] public string? Username { get; init; }
    [Required] public string? Password { get; init; }
    [Required] public string? Database { get; init; }

    public string? ApplicationName { get; init; }

    public int? Timeout { get; init; }

    public bool? TrustServerCertificate { get; set; }

    public bool? Encrypt { get; set; }

    public bool? Pooling { get; set; }
    public int? MinPoolSize { get; set; }
    public int? MaxPoolSize { get; set; }
    
    public ConnectionResult? ConnectionResult { get; set; }

    public ConnectionString()
    {
        
    }
    
    public ConnectionString(string @string)
    {
        var builder = new DbConnectionStringBuilder
        {
            ConnectionString = @string
        };
        if (builder.TryGetValue("data source", out var dataSource))
        {
            Server = dataSource.ToString();
        }
        if (builder.TryGetValue("user id", out var userId))
        {
            Username = userId.ToString();
        }
        if (builder.TryGetValue("password", out var password))
        {
            Password = password.ToString();
        }
        if (builder.TryGetValue("initial catalog", out var initialCatalog))
        {
            Database = initialCatalog.ToString();
        }

        if (builder.TryGetValue("application name", out var appName))
        {
            ApplicationName = builder["application name"].ToString();
        }
        
        if (builder.TryGetValue("integrated security", out var integratedSecurity))
        {
            IntegratedSecurity = bool.Parse(integratedSecurity.ToString()!);
        }
        
        if (builder.TryGetValue("trustservercertificate", out var trustServerCertificate))
        {
            TrustServerCertificate = bool.Parse(trustServerCertificate.ToString()!);
        }
        
        if (builder.TryGetValue("encrypt", out var encrypt))
        {
            Encrypt = bool.Parse(encrypt.ToString()!);
        }
        
        if (builder.TryGetValue("timeout", out var timeout))
        {
            Timeout = int.Parse(timeout.ToString()!);
        }
    }
    
    
    /// <summary>
    /// Returns the string representation of a ConnectionString.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var builder = new DbConnectionStringBuilder
        {
            ["data source"] = Server,
            ["initial catalog"] = Database
        };

        if (IntegratedSecurity)
            builder["integrated security"] = true;
        else
        {
            builder["user id"] = Username;
            builder["password"] = Password;
        }

        if (ApplicationName != null)
            builder["application name"] = ApplicationName;
        
        if (Timeout != null)
            builder["timeout"] = Timeout;
        
        if (TrustServerCertificate != null)
            builder["trustservercertificate"] = TrustServerCertificate;
        
        if (Encrypt != null)
            builder["encrypt"] = Encrypt;
        return builder.ToString();
    }
}