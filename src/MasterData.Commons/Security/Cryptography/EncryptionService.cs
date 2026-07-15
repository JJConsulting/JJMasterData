#nullable enable

using System;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using Microsoft.AspNetCore.DataProtection;

namespace JJMasterData.Commons.Security.Cryptography;

internal sealed class EncryptionService : IEncryptionService
{
    private const string Purpose = "JJMasterData.Commons.Security.Cryptography.EncryptionService";

    private readonly IDataProtector _protector;

    public EncryptionService(IDataProtectionProvider dataProtectionProvider)
    {
        ArgumentNullException.ThrowIfNull(dataProtectionProvider);

        _protector = dataProtectionProvider.CreateProtector(Purpose);
    }

    public string EncryptString(string plainText)
    {
        ArgumentNullException.ThrowIfNull(plainText);

        return _protector.Protect(plainText);
    }

    public string DecryptString(string cipherText)
    {
        ArgumentNullException.ThrowIfNull(cipherText);

        return _protector.Unprotect(cipherText);
    }
}