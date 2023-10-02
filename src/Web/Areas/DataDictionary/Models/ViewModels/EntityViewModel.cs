using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.Events.Abstractions;

namespace JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;

public class EntityViewModel : DataDictionaryViewModel
{
    public FormElement FormElement { get; set; } = null!;
    public IFormEventHandler? FormEvent { get; set; }
    public string FormEventType => IsPythonFormEvent ? "Python" : ".NET";
    public bool IsPythonFormEvent => FormEvent != null && FormEvent.GetType().ToString().Contains('$');
    public bool ReadOnly { get; set; }
    public object HtmlAttributes =>
        ReadOnly ? new { @class = "form-control", disabled="disabled"} : new { @class = "form-control" };

    // ReSharper disable once UnusedMember.Global
    // Reason: Used for model binding.
    public EntityViewModel()
    {
        
    }
    
    public EntityViewModel(string elementName, string menuId) : base(elementName, menuId)
    {
    }
}