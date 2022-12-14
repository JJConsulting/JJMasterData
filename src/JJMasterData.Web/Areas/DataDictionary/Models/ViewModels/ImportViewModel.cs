using JJMasterData.Core.WebComponents;

namespace JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;

public record ImportViewModel(Action<JJUploadArea> Configure);