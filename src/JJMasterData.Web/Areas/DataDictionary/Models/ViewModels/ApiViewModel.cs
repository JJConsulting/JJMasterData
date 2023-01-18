using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;

public class ApiViewModel : DataDictionaryViewModel
{
    public bool IsSync { get; set; }
    public SyncMode Mode { get; set; }
    public List<ElementField> Fields { get; set; } = null!;
    public MetadataApiOptions MetadataApiOptions { get; set; } = null!;
}