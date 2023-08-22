using System.Collections.Generic;
using System.Linq;
using JJMasterData.Core.DataDictionary.Actions.Abstractions;
using JJMasterData.Core.DataDictionary.Actions.GridTable;
using JJMasterData.Core.DataDictionary.Actions.UserCreated;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Actions;


public class GridTableActionList : FormElementActionList
{
    public DeleteAction DeleteAction => (DeleteAction)List.First(a => a.Name == DeleteAction.ActionName);
    public EditAction EditAction => (EditAction)List.First(a => a.Name == EditAction.ActionName);
    public ViewAction ViewAction => (ViewAction)List.First(a => a.Name == ViewAction.ActionName) ;
    public GridTableActionList()
    {
        List.Add(new DeleteAction());
        List.Add(new EditAction());
        List.Add(new ViewAction());
    }
    
    [JsonConstructor]
    private GridTableActionList(IList<BasicAction> list)
    {
        List = list;
    }

    public void Add(UserCreatedAction item)
    {
        ValidateAction(item);
        List.Add(item);
        
    }

}