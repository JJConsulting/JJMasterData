using static System.Environment;

namespace JJMasterData.Web.Models;

public class ErrorViewModel
{
    public int StatusCode { get; set; }
    public string? Message { get; set; }
    public string? Exception { get; set; }
    public string? StackTrace { get; set; }
    public bool IsProduction => GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Development";
}