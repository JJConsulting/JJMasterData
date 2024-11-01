using JJMasterData.Commons.Data.Entity.Models;

namespace JJMasterData.Web.Areas.MasterData.Models;

public class InternalRedirectState
{
    public string? ElementName { get; set; }
    public RelationshipViewType RelationshipType { get; set; } = RelationshipViewType.List;
    public Dictionary<string, object> RelationValues { get; } = new();
}
