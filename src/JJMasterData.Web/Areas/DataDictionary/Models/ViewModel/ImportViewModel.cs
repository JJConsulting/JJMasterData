using JJMasterData.Core.WebComponents;

namespace JJMasterData.Web.Areas.DataDictionary.Models.ViewModel;

public record ImportViewModel(Action<JJUploadArea> Configure);