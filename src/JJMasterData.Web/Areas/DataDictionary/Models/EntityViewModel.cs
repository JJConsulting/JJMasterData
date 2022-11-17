using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.FormEvents;
using JJMasterData.Core.FormEvents.Abstractions;

namespace JJMasterData.Web.Areas.DataDictionary.Models;

public class EntityViewModel : DataDictionaryViewModel
{
    public FormElement? FormElement { get; set; }
    public IFormEvent? FormEvent { get; set; }
    public string FormEventMethods => string.Join(",",FormEventManager.GetFormEventMethods(FormEvent));
    public string FormEventType => IsPythonFormEvent ? "Python" : ".NET";
    public bool IsPythonFormEvent => FormEvent != null && FormEvent.GetType().ToString().Contains('$');
    public bool ReadOnly { get; set; }
    public object HtmlAttributes =>
        ReadOnly ? new { @class = "form-control", @readonly = "readonly" } : new { @class = "form-control" };
}