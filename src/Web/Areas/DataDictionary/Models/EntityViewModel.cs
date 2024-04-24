using System.ComponentModel.DataAnnotations;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.Events.Abstractions;

namespace JJMasterData.Web.Areas.DataDictionary.Models;

public sealed class EntityViewModel : DataDictionaryViewModel
{
    public Entity Entity { get; set; } = null!;
    public IEventHandler? FormEvent { get; set; }
    [Display(Name = "Type")]
    public string FormEventType => IsPythonFormEvent ? "Python" : ".NET";
    public bool IsPythonFormEvent => FormEvent != null && FormEvent.GetType().ToString().Contains('$');
    public bool Disabled { get; init; }

    public string? CurrentTab { get; set; }
    
    // ReSharper disable once UnusedMember.Global
    // Reason: Used for model binding.
    public EntityViewModel()
    {
        
    }
    
    public EntityViewModel(string elementName, string menuId) : base(elementName, menuId)
    {
    }
}