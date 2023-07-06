using JJMasterData.Core.Web.Components;

namespace JJMasterData.Web.Areas.MasterData.Models;

public class LookupViewModel
{
    public required LookupParameters LookupParameters { get; init; }
    public required Action<JJFormView,LookupParameters> LookupFormConfiguration { get; init; }
}