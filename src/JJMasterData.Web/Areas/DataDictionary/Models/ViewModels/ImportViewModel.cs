using JJMasterData.Core.Web.Components;

namespace JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;

public record ImportViewModel(Action<JJUploadArea> Configure);