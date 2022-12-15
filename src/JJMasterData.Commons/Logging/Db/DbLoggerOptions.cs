namespace JJMasterData.Commons.Logging.Db;

public record DbLoggerOptions
{
    public string TableName { get; set; } = "JJMasterDataLogger";
    public string ConnectionStringName { get; set; } = "ConnectionString";
}
