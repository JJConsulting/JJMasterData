using JJMasterData.Commons.DI;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.DataDictionary.Repository;

public static class DictionaryRepositoryFactory
{
    public static IDataDictionaryRepository GetInstance()
    {
        using var scope = JJService.Provider.CreateScope();
        return scope.ServiceProvider.GetService<IDataDictionaryRepository>();
    }
}
