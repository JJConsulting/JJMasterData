using System.ComponentModel.DataAnnotations;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.Events.Abstractions;

namespace JJMasterData.Web.Areas.DataDictionary.Models;

public sealed class EntityViewModel
{
    public required string ElementName { get; set; }
    public Entity Entity { get; set; } = null!;
    public IEventHandler? FormEvent { get; set; }
    [Display(Name = "Type")]
    public string FormEventType => IsPythonFormEvent ? "Python" : ".NET";
    public bool IsPythonFormEvent => FormEvent != null && FormEvent.GetType().ToString().Contains('$');
    public bool Disabled { get; init; }
}