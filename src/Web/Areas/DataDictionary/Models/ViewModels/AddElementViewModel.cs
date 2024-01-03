using System.ComponentModel.DataAnnotations;

namespace JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;

public class AddElementViewModel
{
    [Display(Name = "Table Or View Name", Prompt = "Table Or View Name")]
    public string Name { get; set; } = null!;
    public bool ImportFields { get; set; }
}