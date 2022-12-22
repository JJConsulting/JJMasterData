using JJMasterData.Commons.Dao;
using JJMasterData.Core.DataDictionary.Repository;

namespace JJMasterData.Core.Facades;

public class RepositoryServicesFacade
{
    public IDataDictionaryRepository DataDictionaryRepository { get; }
    public IEntityRepository EntityRepository { get; }

    public RepositoryServicesFacade(
        IDataDictionaryRepository dataDictionaryRepository,
        IEntityRepository entityRepository)
    {
        DataDictionaryRepository = dataDictionaryRepository;
        EntityRepository = entityRepository;
    }
}