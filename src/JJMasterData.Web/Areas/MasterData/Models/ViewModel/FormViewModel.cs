using JJMasterData.Core.WebComponents;

namespace JJMasterData.Web.Areas.MasterData.Models.ViewModel;

public record FormViewModel(string DictionaryName, Action<JJFormView> ConfigureFormView);