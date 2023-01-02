using JJMasterData.Commons.Dao;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.WebComponents.Factories;

public class SearchBoxFactory
{
    private readonly IHttpContext _httpContext;
    private readonly IEntityRepository _entityRepository;
    private readonly ILoggerFactory _loggerFactory;
    public SearchBoxFactory(IHttpContext httpContext, IEntityRepository entityRepository, ILoggerFactory loggerFactory)
    {
        _httpContext = httpContext;
        _entityRepository = entityRepository;
        _loggerFactory = loggerFactory;
    }

    public JJSearchBox CreateSearchBox()
    {
        return new JJSearchBox(_httpContext, _entityRepository, _loggerFactory);
    }
}