using JJMasterData.Commons.Security.Cryptography;

namespace JJMasterData.Commons.Test.Security;

public class AesEncryptionTest
{
    [Theory]
    [InlineData("r9/COvUnoHgv6wLnbtj2Lg==")]
    public void AesDecryptTest(string content)
    {
        var service = new AesEncryptionAlgorithm();
        string descripted = service.DecryptString(content,"Example");
        Assert.Equal("JJMasterData", descripted);
    }

    [Theory]
    [InlineData("JJMasterData")]
    public void AesEncryptTest(string content)
    {
        var service = new AesEncryptionAlgorithm();
        string encripted = service.EncryptString(content,"Example");
        Assert.Equal("r9/COvUnoHgv6wLnbtj2Lg==", encripted);
    }

}