using JJMasterData.Commons.Util;

namespace JJMasterData.Commons.Test.Util;

public class FormatTest
{
    [Fact]
    public void FormatTimeSpanTest()
    {
        string text = Format.FormatTimeSpan(DateTime.Now.AddSeconds(-62), DateTime.Now);
        const string expected = "1 minuto e 2 segundos";

        Assert.Equal(expected, text);
    }
}