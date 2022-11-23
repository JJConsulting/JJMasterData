using JJMasterData.Commons.DI;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.DataDictionary.Repository;

public static class DictionaryRepositoryFactory
{
    public static IDictionaryRepository GetInstance()
    {
        using var scope = JJService.Provider.CreateScope();
        return scope.ServiceProvider.GetService<IDictionaryRepository>();
    }
}
