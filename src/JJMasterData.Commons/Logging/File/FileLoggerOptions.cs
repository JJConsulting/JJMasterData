namespace JJMasterData.Commons.Logging.File;

public record FileLoggerOptions
{
    public string FileName { get; set; } = "Log/AppLog-yyyyMMdd.txt";
}
