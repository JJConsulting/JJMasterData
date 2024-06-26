using JJMasterData.Commons.Data.Entity.Models;

namespace JJMasterData.Web.Areas.DataDictionary.Models;

public sealed class ElementScriptsViewModel : DataDictionaryViewModel
{
    public required ScriptsResult Scripts { get; init; }
    public bool TableExists { get; set; }

    public ElementScriptsViewModel()
    {
        
    }
}