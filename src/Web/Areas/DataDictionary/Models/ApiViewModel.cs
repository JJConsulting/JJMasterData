using System.ComponentModel.DataAnnotations;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.Web.Areas.DataDictionary.Models;

public sealed class ApiViewModel : DataDictionaryViewModel
{
    [Display(Name = "Enable Synchronism")]
    public bool EnableSynchronism { get; set; }
    
    [Display(Name = "Mode")]
    public SynchronismMode SynchronismMode { get; set; }
    public List<ElementField>? ElementFields { get; set; } 
    public FormElementApiOptions ApiOptions { get; set; } = null!;
    
    // ReSharper disable once UnusedMember.Global
    // Reason: Used for model binding.
    public ApiViewModel()
    {
        
    }
    public ApiViewModel(string elementName, string menuId) : base(elementName, menuId)
    {
    }
}