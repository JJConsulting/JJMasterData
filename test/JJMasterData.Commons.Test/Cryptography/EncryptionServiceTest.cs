using JJMasterData.Commons.Cryptography;

namespace JJMasterData.Commons.Test.Cryptography;

public class EncryptionServiceTest
{
    [Theory]
    [InlineData("gustavo","key")]
    [InlineData("0000000000042","betterKey")]
    [InlineData("1","a")]
    [InlineData("JJMasterData","superMasterKey")]
    [InlineData("Mf24PjvPzgY7QhASOgK9yYRPUPmVsthDK4KmdBrzzDJrxlnixBP3NpcR2mwkhXNURcadApj2mR/1UOXz7PyQKGVocsIKzeHODbkC1qSRr0=","ChangeMe")]
    public void AesEncryptionServiceTest(string text, string key)
    {
        var service = new AesEncryptionService();

        var encrypted = service.EncryptString(text, key);

        var unencrypted = service.DecryptString(encrypted, key);
        
        Assert.Equal(text, unencrypted);
    }
}