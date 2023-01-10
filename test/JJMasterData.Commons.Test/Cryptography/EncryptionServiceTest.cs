using JJMasterData.Commons.Cryptography;

namespace JJMasterData.Commons.Test.Cryptography;

public class EncryptionServiceTest
{
    [Theory]
    [InlineData("key","gustavo")]
    [InlineData("betterKey","0000000000042")]
    [InlineData("a","1")]
    [InlineData("superMasterKey","JJMasterData")]
    public void AesEncryptionServiceTest(string key, string text)
    {
        var service = new AesEncryptionService();

        var encrypted = service.EncryptString(text, key);

        var unencrypted = service.DecryptString(encrypted, key);
        
        Assert.Equal(text, unencrypted);
    }
}