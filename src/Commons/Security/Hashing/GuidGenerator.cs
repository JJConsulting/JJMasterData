using System;

namespace JJMasterData.Commons.Hashing;

public static class GuidGenerator
{
    public static Guid FromValue(string value)
    {
        return new Guid(Md5HashHelper.ComputeHash(value));
    }
}