using JJMasterData.Core.DataDictionary.Models.Actions;

namespace JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;

public class ActionsListViewModel : DataDictionaryViewModel
{
    public required List<BasicAction> GridTableActions { get; init; }
    public required List<BasicAction> GridToolbarActions { get;  init; }
    public required List<BasicAction> FormToolbarActions { get;  init; }
    public ActionsListViewModel(string elementName, string menuId) : base(elementName, menuId)
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