using System.Collections.Generic;
using System.Linq;
using JJMasterData.Core.DataDictionary.Actions.Abstractions;
using JJMasterData.Core.DataDictionary.Actions.GridTable;
using JJMasterData.Core.DataDictionary.Actions.GridToolbar;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Actions;


public class GridTableActionList : FormElementActionList
{
    public DeleteAction DeleteAction => List.First(a => a.Name == DeleteAction.ActionName) as DeleteAction;
    public EditAction EditAction => List.First(a => a.Name == EditAction.ActionName) as EditAction;
    public InsertAction InsertAction => List.First(a => a.Name == InsertAction.ActionName) as InsertAction;
    public ViewAction ViewAction => List.First(a => a.Name == ViewAction.ActionName) as ViewAction;
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
}