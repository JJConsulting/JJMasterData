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

    [Theory]
    [InlineData("19131243000197", "19.131.243/0001-97")]
    [InlineData("AB.CDE.FGH/IJKL-52", "AB.CDE.FGH/IJKL-52")]
    public void FormatCnpj_ShouldSupportNumericAndAlphanumeric(string input, string expected)
    {
        var result = Format.FormatCnpj(input);

        Assert.Equal(expected, result);
    }
}
