using System.ComponentModel.DataAnnotations;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public interface ISubmittableAction
{
    [Display(Name = "Is Submit")]
    public bool IsSubmit { get; set; }
}