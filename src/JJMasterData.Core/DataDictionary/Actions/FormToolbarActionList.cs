using System.Linq;
using JJMasterData.Core.DataDictionary.Action;
using JJMasterData.Core.DataDictionary.Action.Form;

namespace JJMasterData.Core.DataDictionary;

public class FormToolbarActionList : FormElementActionList
{
    public CancelAction CancelAction => List.First(a => a.Name == CancelAction.Name) as CancelAction;
    public InsertAction InsertAction => List.First(a => a.Name == InsertAction.Name) as InsertAction;
    
    public FormToolbarActionList()
    {
        List.Add(new InsertAction());
        List.Add(new CancelAction());
    }
}