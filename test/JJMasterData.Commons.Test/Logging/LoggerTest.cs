using JJMasterData.Commons.Logging;

namespace JJMasterData.Commons.Test.Logging;

public class LoggerTest
{
    [Fact]
    public void Log_Test()
    {
        Log.AddInfo("Test");
        Assert.True(true);
    }

}