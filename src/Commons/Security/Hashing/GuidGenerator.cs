using System;
using JJMasterData.Commons.Hashing;

namespace JJMasterData.Commons.Security.Hashing;

public static class GuidGenerator
{
    public static Guid FromValue(string value)
    {
        return new Guid(Md5HashHelper.ComputeHash(value));
    }
}