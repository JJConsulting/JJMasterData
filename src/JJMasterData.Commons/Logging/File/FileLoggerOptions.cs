namespace JJMasterData.Commons.Logging.File;

public class FileLoggerOptions
{
    public string FileName { get; set; } = "Log/AppLog-yyyyMMdd.txt";
    public FileLoggerFormatting Formatting { get; set; }
}
