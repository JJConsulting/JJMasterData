using JJMasterData.Commons.Logging.Db;
using JJMasterData.Commons.Logging.File;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace JJMasterData.Commons.Test.Logging;

public class LoggerTest
{
    [Fact]
    public void LogAtFileTest()
    {
        var buffer = new Mock<FileLoggerBuffer>();
        var options = new Mock<IOptionsMonitor<FileLoggerOptions>>();
        var fileLogger = new FileLogger("TestCategory",buffer.Object,options.Object);
        fileLogger.LogInformation("Test");
        
        var exception = Record.Exception(() => fileLogger.LogInformation("Test"));
        
        Assert.Null(exception);
    }
    
    [Fact]
    public void LogAtDatabaseTest()
    {
        var buffer = new Mock<DbLoggerBuffer>();
        var dbLogger = new DbLogger("TestCategory",buffer.Object);
        
        var exception = Record.Exception(() => dbLogger.LogInformation("Test"));
        Assert.Null(exception);
    }

}