namespace JJMasterData.Commons.Data.Entity.Providers;

public class SqlServerOptions
{
    /// <summary>
    /// Specifies the compatibility level for SQL Server.
    /// The value determines the behavior of SQL script generation
    /// and SQL Server syntax compatibility.
    /// </summary>
    public int CompatibilityLevel { get; set; } = 160;
}