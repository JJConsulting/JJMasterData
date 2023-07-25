using JJMasterData.Commons.Logging;
using JJMasterData.Commons.Logging.Db;
using JJMasterData.Commons.Logging.File;
using Microsoft.Extensions.Logging;
using Moq;

namespace JJMasterData.Commons.Test.Logging;

public class LoggerTest
{
    [Fact]
    public void LogAtFileTest()
    {
        var mock = new Mock<FileLoggerBuffer>();
        var fileLogger = new FileLogger(mock.Object);
        fileLogger.LogInformation("Test");
        
        var exception = Record.Exception(() => fileLogger.LogInformation("Test"));
        
        Assert.Null(exception);
    }
    
    [Fact]
    public void LogAtDatabaseTest()
    {
        var mock = new Mock<DbLoggerBuffer>();
        var dbLogger = new DbLogger(mock.Object);
        
        var exception = Record.Exception(() => dbLogger.LogInformation("Test"));
        Assert.Null(exception);
    }

}