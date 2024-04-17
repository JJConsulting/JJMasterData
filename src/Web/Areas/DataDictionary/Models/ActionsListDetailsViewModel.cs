using JJMasterData.Core.DataDictionary.Models.Actions;

namespace JJMasterData.Web.Areas.DataDictionary.Models;

public sealed class ActionsListDetailsViewModel : DataDictionaryViewModel
{
    public required ActionSource Source { get; init; }
    public required List<BasicAction> Actions { get; init; }
    public string? FieldName { get; init; }
    
#pragma warning disable CS8618
    // ReSharper disable once UnusedMember.Global
    // Reason: Used for model binding.
    public ActionsListDetailsViewModel()
#pragma warning restore CS8618
    {
        
    }
}