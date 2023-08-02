using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager.Models;

namespace JJMasterData.Core.DataManager.Services;

public interface IUrlRedirectService
{
    Task<UrlRedirectModel> GetUrlRedirectAsync(FormElement formElement,ActionMap actionMap, PageState pageState);
}