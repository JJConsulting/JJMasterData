using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JJMasterData.Web.Filters;

public class ServiceFilterAttribute<TFilter>() : ServiceFilterAttribute(typeof(TFilter))
    where TFilter : IFilterMetadata;
