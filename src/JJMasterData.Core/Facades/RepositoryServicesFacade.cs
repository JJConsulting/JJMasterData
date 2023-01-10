using JJMasterData.Commons.Dao.Entity.Abstractions;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;

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