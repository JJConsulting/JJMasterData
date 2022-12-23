using JJMasterData.Commons.Dao;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.WebComponents.Factories;

public class SearchBoxFactory
{
    private readonly IHttpContext _httpContext;
    private readonly IEntityRepository _entityRepository;

    public SearchBoxFactory(IHttpContext httpContext, IEntityRepository entityRepository)
    {
        _httpContext = httpContext;
        _entityRepository = entityRepository;
    }

    public JJSearchBox CreateSearchBox()
    {
        return new JJSearchBox(_httpContext, _entityRepository);
    }
}