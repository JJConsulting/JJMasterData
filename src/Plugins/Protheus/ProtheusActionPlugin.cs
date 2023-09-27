using JJMasterData.Commons.Security.Hashing;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.Web.Html;

namespace JJMasterData.Protheus;

public class ProtheusActionPlugin : IActionPlugin
{
    public Guid Id => GuidGenerator.FromValue(nameof(ProtheusActionPlugin));
    public string Title => "Protheus";

    public IEnumerable<string> AdditionalParametersHints
    {
        get
        {
            yield return "Url";
        }
    }
    public HtmlBuilder? AdditionalInformationHtml => null;
    public async Task<PluginActionResult> ExecuteActionAsync(IDictionary<string, object?> values)
    {
        await Task.Delay(1);
        values["NomePedido"] = "oi do protheus";
        return new PluginActionResult()
        {
            JsCallback = "alert('pan')"
        };
    }
}