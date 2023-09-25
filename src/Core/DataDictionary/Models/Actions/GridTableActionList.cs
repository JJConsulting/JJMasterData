using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Actions;


public class GridTableActionList : FormElementActionList
{
    public DeleteAction DeleteAction => List.First(a => a is DeleteAction) as DeleteAction;
    public EditAction EditAction => List.First(a => a is EditAction) as EditAction;
    public ViewAction ViewAction => List.First(a => a is ViewAction) as ViewAction;
    public GridTableActionList()
    {
        List.Add(new DeleteAction());
        List.Add(new EditAction());
        List.Add(new ViewAction());
    }
    
    [JsonConstructor]
    private GridTableActionList(List<BasicAction> list)
    {
        List = list;
    }

    public void Add(UserCreatedAction item)
    {
        ValidateAction(item);
        List.Add(item);
        
    }

}