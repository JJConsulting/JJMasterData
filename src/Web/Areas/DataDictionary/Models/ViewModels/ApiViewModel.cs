using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;

public class ApiViewModel : DataDictionaryViewModel
{
    public bool IsSync { get; set; }
    public SyncMode Mode { get; set; }
    public List<ElementField>? Fields { get; set; } 
    public FormElementApiOptions MetadataApiOptions { get; set; } = null!;
    
    // ReSharper disable once UnusedMember.Global
    // Reason: Used for model binding.
    public ApiViewModel()
    {
        
    }
    public ApiViewModel(string elementName, string menuId) : base(elementName, menuId)
    {
    }
}