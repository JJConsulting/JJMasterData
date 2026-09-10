using Microsoft.AspNetCore.DataProtection;

namespace JJMasterData.Commons.Security;

public sealed class DataProtectionService(IDataProtectionProvider dataProtectionProvider)
{
    private const string Purpose = "JJConsulting.MasterData.Commons.Security";
    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector(Purpose);

    public string Protect(string plainText)
    {
        return _protector.Protect(plainText);
    }

    public string Unprotect(string protectedData)
    {
        return _protector.Unprotect(protectedData);
    }
}