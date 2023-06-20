namespace JJMasterData.Commons.Logging.File;

public enum FileLoggerFormatting
{
    /// <summary>
    /// Designed to provide a clean and easily readable output.
    /// </summary>
    Default,
    /// <summary>
    /// A more compact logging format inspired by structured logging.
    /// </summary>
    Compact,
    /// <summary>
    /// Store the logs at JSON objects.
    /// </summary>
    Json
}