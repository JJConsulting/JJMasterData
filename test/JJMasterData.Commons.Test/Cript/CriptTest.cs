namespace JJMasterData.Commons.Test.Cript;

public class CriptTest
{
    [Theory]
    [InlineData("JJMasterData")]
    public void EnigmaEncryptRPTest(string content)
    {
        string encripted = Commons.Util.Cript.EnigmaEncryptRP(content);

        Assert.Equal("AFADBFC6E7C7CAD5B6C6E8B4", encripted);
    }
            

    [Theory]
    [InlineData("AFADBFC6E7C7CAD5B6C6E8B4")]
    public void EnigmaDecryptRPTest(string content)
    {
        string descripted = Commons.Util.Cript.EnigmaDecryptRP(content);

        Assert.Equal("JJMasterData", descripted);
    }


    [Theory]
    [InlineData("r9/COvUnoHgv6wLnbtj2Lg==")]
    public void Decript64Test(string content)
    {
        string descripted = Commons.Util.Cript.Descript64(content);
        Assert.Equal("JJMasterData", descripted);
    }

    [Theory]
    [InlineData("JJMasterData")]
    public void Cript64Test(string content)
    {
        string encripted = Commons.Util.Cript.Cript64(content);
        Assert.Equal("r9/COvUnoHgv6wLnbtj2Lg==", encripted);
    }

}