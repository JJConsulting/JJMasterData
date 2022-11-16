using System.ComponentModel.DataAnnotations;

namespace JJMasterData.Web.Models;

public class ConnectionString
{
    [Required]
    public string Server { get; init; }
    [Required] 
    public string Username { get; init; }
    [Required]
    public string Password { get; init; }
    [Required]
    public string Database { get; init; }
    
    public static ConnectionString FromString(string @string)
    {
        var builder = new System.Data.Common.DbConnectionStringBuilder
        {
            ConnectionString = @string
        };
        var connection = new ConnectionString
        {
            Server = builder["data source"].ToString()!,
            Username = builder["user id"].ToString()!,
            Password = builder["password"].ToString()!,
            Database = builder["initial catalog"].ToString()!
        };
        return connection;
    }
    
    public ConnectionString()
    {
        
    }
}