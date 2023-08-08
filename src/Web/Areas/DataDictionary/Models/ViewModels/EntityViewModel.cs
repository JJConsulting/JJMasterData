using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.FormEvents.Abstractions;

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
    
    public EntityViewModel(string dictionaryName, string menuId) : base(dictionaryName, menuId)
    {
    }
}