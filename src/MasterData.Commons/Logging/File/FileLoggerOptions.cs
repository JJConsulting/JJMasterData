using System;

namespace JJMasterData.Commons.Logging.File;


[Obsolete("Please use Serilog.")]
public sealed class FileLoggerOptions : BatchingLoggerOptions
{
    private int? _fileSizeLimit = 10 * 1024 * 1024;
    private int? _retainedFileCountLimit = 2;

    /// <summary>
    /// Gets or sets a strictly positive value representing the maximum log size in bytes or null for no limit.
    /// Once the log is full, no more messages will be appended.
    /// Defaults to <c>10MB</c>.
    /// </summary>
    public int? FileSizeLimit
    {
        get => _fileSizeLimit;
        set
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), @$"{nameof(FileSizeLimit)} must be positive.");
            }
            _fileSizeLimit = value;
        }
    }

    /// <summary>
    /// Gets or sets a strictly positive value representing the maximum retained file count or null for no limit.
    /// Defaults to <c>2</c>.
    /// </summary>
    public int? RetainedFileCountLimit
    {
        get => _retainedFileCountLimit;
        set
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), @$"{nameof(RetainedFileCountLimit)} must be positive.");
            }
            _retainedFileCountLimit = value;
        }
    }

    /// <summary>
    /// Gets or sets a string representing the file name used to store the logging information.
    /// </summary>
    public string FileName { get; set; } = "Log/AppLog-yyyyMMdd.txt";
    
    public FileLoggerFormatting Formatting { get; set; } = FileLoggerFormatting.Default;
}
