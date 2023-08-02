using Newtonsoft.Json;

namespace JJMasterData.Core.DataManager.Models;

public record UrlRedirectModel(bool UrlAsPopUp, string PopUpTitle, string UrlRedirect)
{
    public string ToJson() => JsonConvert.SerializeObject(this);
}