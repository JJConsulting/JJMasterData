using Microsoft.Extensions.Configuration;

namespace JJMasterData.Web.Areas.MasterData.Models;

/// <summary>
/// Data representation of the ConnectionStrings of <see cref="IConfiguration" />
/// </summary>
public record ConnectionStrings
{
    public string? ConnectionString { get; set; }
}