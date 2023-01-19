using JJMasterData.Core.Web.Components;

namespace JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;

public class DataDictionaryViewModel
{
    public string? MenuId { get; set; }

    public string? DictionaryName { get; set; }
    
    public JJValidationSummary? ValidationSummary { get; set; }

    protected DataDictionaryViewModel()
    {
        
    }
    
    public DataDictionaryViewModel(string dictionaryName, string menuId)
    {
        DictionaryName = dictionaryName;
        MenuId = menuId;
    }
}