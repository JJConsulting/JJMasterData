using JJMasterData.Commons.Security.Hashing;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.Web.Html;

namespace JJMasterData.WebEntryPoint;

public class ProtheusActionPlugin : IActionPlugin
{
    public Guid Id => GuidGenerator.FromValue(nameof(ProtheusActionPlugin));
    public string Title => "Protheus";
    public HtmlBuilder? AdditionalInformationHtml => null;

    public Task<PluginActionResult> ExecuteActionAsync(IDictionary<string, object> values)
    {
        throw new NotImplementedException();
    }
}