using System;

namespace JJMasterData.Commons.Background;

internal sealed class MasterDataProgress<T>(Action<T> report) : IProgress<T>
{
    public void Report(T value) => report(value);
}