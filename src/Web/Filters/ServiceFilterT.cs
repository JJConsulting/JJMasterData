using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Filters;

public class ServiceFilter<T> : ServiceFilterAttribute
{
    public ServiceFilter() : base(typeof(T))
    {
    }
}