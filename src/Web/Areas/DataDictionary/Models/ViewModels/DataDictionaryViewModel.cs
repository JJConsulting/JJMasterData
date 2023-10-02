using JJMasterData.Core.UI.Components;

namespace JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;

public class DataDictionaryViewModel
{
    public string MenuId { get; set; }

    public string ElementName { get; set; }
    
    public JJValidationSummary? ValidationSummary { get; set; }

#pragma warning disable CS8618
    // ReSharper disable once UnusedMember.Global
    // Reason: Used for model binding.
    protected DataDictionaryViewModel()
#pragma warning restore CS8618
    {
        
    }
    
    public DataDictionaryViewModel(string elementName, string menuId)
    {
        ElementName = elementName;
        MenuId = menuId;
    }
}