using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataDictionary.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;

namespace JJMasterData.Core.Test.DataDictionary.Services;

internal static class DataDictionaryTestHelper
{
    public static DataDictionaryService Create(IDataDictionaryRepository repository)
    {
        var options = new Mock<IOptionsSnapshot<MasterDataCoreOptions>>();
        options.SetupGet(x => x.Value).Returns(new MasterDataCoreOptions { EnableDataDictionaryCaching = false });
        return new DataDictionaryService(repository, new MemoryCache(new MemoryCacheOptions()), options.Object);
    }
}
