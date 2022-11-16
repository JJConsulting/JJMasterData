using System.ComponentModel.DataAnnotations;

namespace JJMasterData.Web.Models;

public class ConnectionString
{
    [Required]
    public string Server { get; init; }

    public bool IntegratedSecurity { get; set; } = true;
    
    [Required] 
    public string Username { get; init; }
    [Required]
    public string Password { get; init; }
    [Required]
    public string Database { get; init; }
    
    public string? ApplicationName { get; init; }
    
    public int? Timeout { get; init; }
    
    public bool TrustServerCertificate { get; set; }
    
    public bool Encrypt { get; set; }
    
    public bool Pooling { get; set; }
    public int? MinPoolSize { get; set; }
    public int? MaxPoolSize { get; set; }
    public ConnectionString(string @string)
    {
        var builder = new System.Data.Common.DbConnectionStringBuilder
        {
            ConnectionString = @string
        };
  
        Server = builder["data source"].ToString()!;
        Username = builder["user id"].ToString()!;
        Password = builder["password"].ToString()!;
        Database = builder["initial catalog"].ToString()!;
    }
}