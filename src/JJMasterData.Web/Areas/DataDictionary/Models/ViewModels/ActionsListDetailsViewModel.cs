using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Actions.Abstractions;

namespace JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;

public class ActionsListDetailsViewModel : DataDictionaryViewModel
{
    public required ActionSource Context { get; set; }
    public required List<BasicAction> Actions { get; init; }
    public string? FieldName { get; set; }
}