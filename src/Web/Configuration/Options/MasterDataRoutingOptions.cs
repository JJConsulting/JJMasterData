using System.Globalization;

namespace JJMasterData.Web.Configuration.Options;

public class MasterDataRoutingOptions
{
    public string DefaultCulture { get; set; } = "en-US";
    public List<CultureInfo> AdditionalCultures { get; } = new();
}