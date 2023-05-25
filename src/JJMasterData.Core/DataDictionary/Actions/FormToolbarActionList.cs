using System.Linq;
using JJMasterData.Core.DataDictionary.Action;
using JJMasterData.Core.DataDictionary.Action.Form;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary;

public class FormToolbarActionList : FormElementActionList
{
    [JsonIgnore] public CancelAction CancelAction => List.First(a => a.Name == CancelAction.Name) as CancelAction;
    [JsonIgnore] public SaveAction SaveAction => List.First(a => a.Name == SaveAction.Name) as SaveAction;

    public FormToolbarActionList()
    {
        List.Add(new SaveAction());
        List.Add(new CancelAction());
    }
}