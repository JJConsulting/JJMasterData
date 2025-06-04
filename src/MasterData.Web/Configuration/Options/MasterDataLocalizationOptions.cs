using System.Globalization;

namespace JJMasterData.Web.Configuration.Options;

public sealed class MasterDataLocalizationOptions
{
    public string DefaultCulture { get; set; } = "en-US";
    public List<CultureInfo> AdditionalCultures { get; } = [];
}