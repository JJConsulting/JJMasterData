using System.Collections.Generic;
using System.Linq;
using JJMasterData.Core.DataDictionary.Actions.Abstractions;
using JJMasterData.Core.DataDictionary.Actions.FormToolbar;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Actions;

public class FormToolbarActionList : FormElementActionList
{
    public CancelAction CancelAction => (CancelAction)List.First(a => a.Name == CancelAction.Name);
    public SaveAction SaveAction => (SaveAction)List.First(a => a.Name == SaveAction.Name) ;
    public BackAction BackAction => (BackAction)List.First(a => a.Name == BackAction.Name) ;
    
    public FormToolbarActionList() 
    {
        List.Add(new SaveAction());
        List.Add(new CancelAction());
        List.Add(new BackAction());
    }
    
    [JsonConstructor]
    private FormToolbarActionList(IList<BasicAction> list)
    {
        List = list;
    }
}