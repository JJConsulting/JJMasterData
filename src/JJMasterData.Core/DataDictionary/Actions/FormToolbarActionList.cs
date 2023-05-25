using System.Collections.Generic;
using System.Linq;
using JJMasterData.Core.DataDictionary.Actions.Abstractions;
using JJMasterData.Core.DataDictionary.Actions.FormToolbar;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Actions;

public class FormToolbarActionList : FormElementActionList
{
    public CancelAction CancelAction => List.First(a => a.Name == CancelAction.Name) as CancelAction;
    public SaveAction SaveAction => List.First(a => a.Name == SaveAction.Name) as SaveAction;

    public FormToolbarActionList() 
    {
        List.Add(new SaveAction());
        List.Add(new CancelAction());
    }
    
    [JsonConstructor]
    private FormToolbarActionList(IList<BasicAction> list)
    {
        List = list;
    }
}