using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JJMasterData.Web.Filters;

/// Copied from https://github.com/dotnet/aspnetcore/pull/47075/files#diff-a8bccfcb9686861a7fd1dc4416fe4b9c2a0cba99994a111b259ee675c67ee830
/// When .NET 8 is released, we can remove this class.
public class ServiceFilterAttribute<TFilter> : ServiceFilterAttribute where TFilter : IFilterMetadata
{
    public ServiceFilterAttribute() : base(typeof(TFilter))
    {
    }
}