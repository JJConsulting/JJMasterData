namespace JJMasterData.Commons.Logging.File;

public class FileLoggerOptions(string fileName)
{
    public string FileName { get; set; } = fileName;

    public FileLoggerFormatting Formatting { get; set; }

    public FileLoggerOptions() : this("Log/AppLog-yyyyMMdd.txt")
    {
    }
}
