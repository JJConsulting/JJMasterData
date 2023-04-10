
namespace JJMasterData.Web.Areas.Tools.Models;

/// <summary>
/// Data representation of the ConnectionStrings of Microsoft.Extensions.Configuration.IConfiguration />
/// </summary>
public record ConnectionStrings
{
    public string? ConnectionString { get; set; }
}