#nullable enable
using System;

namespace JJMasterData.Commons.Security.Cryptography.Abstractions;

[Obsolete("Please use Microsoft Data Protection Provider")]
public interface IEncryptionService
{
    string EncryptString(string plainText);
    string DecryptString(string cipherText);
}