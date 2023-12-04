namespace JJMasterData.Commons.Logging.File;

public class FileLoggerOptions
{
    public string FileName { get; set; }

    public FileLoggerFormatting Formatting { get; set; }

    public FileLoggerOptions()
    {
        FileName = "Log/AppLog-yyyyMMdd.txt";
    }

    public FileLoggerOptions(string fileName)
    {
        FileName = fileName;
    }

}
