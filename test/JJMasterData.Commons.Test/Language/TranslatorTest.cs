using System.Globalization;
using JJMasterData.Commons.Language;

namespace JJMasterData.Commons.Test.Language;

public class TranslatorTest
{
    [Fact]
    public void Translate_Test()
    {
        var cultureInfo = new CultureInfo("pt-BR");
        
        CultureInfo.CurrentCulture = cultureInfo;
        CultureInfo.CurrentUICulture = cultureInfo;
        Thread.CurrentThread.CurrentCulture = cultureInfo;
        Thread.CurrentThread.CurrentUICulture = cultureInfo;
        
        Assert.Equal("Objeto", Translate.Key("Object"));
    }

}