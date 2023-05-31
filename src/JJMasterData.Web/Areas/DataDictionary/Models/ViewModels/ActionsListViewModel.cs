using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Actions.Abstractions;

namespace JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;

public class ActionsListViewModel : DataDictionaryViewModel
{
    public required List<BasicAction> GridTableActions { get; init; }
    public required List<BasicAction> GridToolbarActions { get;  init; }
    public required List<BasicAction> FormToolbarActions { get;  init; }
    public ActionsListViewModel(string dictionaryName, string menuId) : base(dictionaryName, menuId)
    {
    }
    
#pragma warning disable CS8618
    // ReSharper disable once UnusedMember.Global
    // Reason: Used for model binding.
    public ActionsListViewModel()
#pragma warning restore CS8618
    {
        
    }
}