using System.Globalization;
using JJMasterData.Commons.Localization;

namespace JJMasterData.Commons.Test.Localization;

public class TranslatorTest
{
    [Fact]
    public void Translate_Test()
    {
        var cultureInfo = new CultureInfo("en-US");
        
        CultureInfo.CurrentCulture = cultureInfo;
        CultureInfo.CurrentUICulture = cultureInfo;
        Thread.CurrentThread.CurrentCulture = cultureInfo;
        Thread.CurrentThread.CurrentUICulture = cultureInfo;
        
        Assert.Equal("Object", Translate.Key("Object"));
    }

}