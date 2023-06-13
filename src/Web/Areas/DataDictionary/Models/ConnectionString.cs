using System.ComponentModel.DataAnnotations;
using System.Data.Common;

namespace JJMasterData.Web.Areas.DataDictionary.Models;

public class ConnectionString
{
    [Required] public string? Server { get; init; }

    public bool IntegratedSecurity { get; init; }

    public string? Username { get; init; }
    
    public string? Password { get; init; }

    [Required] public string? Database { get; init; }

    public string? ApplicationName { get; init; }

    public int? Timeout { get; init; }

    public bool? TrustServerCertificate { get; init; }

    public bool? Encrypt { get; init; }

    public bool? Pooling { get; init; }

    public int? MinPoolSize { get; init; }

    public int? MaxPoolSize { get; init; }

    public ConnectionString()
    {
    }

    public ConnectionString(string? connectionString)
    {
        var builder = new DbConnectionStringBuilder
        {
            ConnectionString = connectionString
        };
        if (builder.TryGetValue("data source", out var dataSource))
        {
            Server = dataSource.ToString();
        }

        if (builder.TryGetValue("user", out var userId))
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
            ApplicationName = appName.ToString();
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

        if (builder.TryGetValue("pooling", out var pooling))
        {
            Pooling = bool.Parse(pooling.ToString()!);
        }

        if (builder.TryGetValue("min pool size", out var minPoolSize))
        {
            MinPoolSize = int.Parse(minPoolSize.ToString()!);
        }

        if (builder.TryGetValue("max pool size", out var maxPoolSize))
        {
            MaxPoolSize = int.Parse(maxPoolSize.ToString()!);
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
            builder["user"] = Username;
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

        if (Pooling != null)
            builder["pooling"] = Pooling;

        if (MinPoolSize != null)
            builder["min pool size"] = MinPoolSize;

        if (MaxPoolSize != null)
            builder["max pool size"] = MaxPoolSize;

        return builder.ToString();
    }
}