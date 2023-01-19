using JJMasterData.Core.Web.Components;

namespace JJMasterData.Web.Areas.MasterData.Models.ViewModels;

public record FormViewModel(string DictionaryName, Action<JJFormView> Configure);