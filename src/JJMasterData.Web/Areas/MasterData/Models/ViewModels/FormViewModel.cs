using JJMasterData.Core.WebComponents;

namespace JJMasterData.Web.Areas.MasterData.Models.ViewModels;

public record FormViewModel(string DictionaryName, Action<JJFormView> Configure);