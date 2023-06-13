
namespace JJMasterData.Web.Areas.DataDictionary.Models;

/// <summary>
/// Data representation of the ConnectionStrings of Microsoft.Extensions.Configuration.IConfiguration />
/// </summary>
public record ConnectionStrings
{
    public string? ConnectionString { get; set; }
}