using System.Linq;
using System.Text.Json.Serialization;
using JJMasterData.Core.DataDictionary.Action;

namespace JJMasterData.Core.DataDictionary;

public class GridTableActionList : FormElementActionList
{
    [JsonIgnore]
    public DeleteAction DeleteAction => List.First(a => a.Name == DeleteAction.ActionName) as DeleteAction;
    [JsonIgnore]
    public EditAction EditAction => List.First(a => a.Name == EditAction.ActionName) as EditAction;
    [JsonIgnore]
    public InsertAction InsertAction => List.First(a => a.Name == InsertAction.ActionName) as InsertAction;
    [JsonIgnore]
    public ViewAction ViewAction => List.First(a => a.Name == ViewAction.ActionName) as ViewAction;
    public GridTableActionList()
    {
        List.Add(new DeleteAction());
        List.Add(new EditAction());
        List.Add(new ViewAction());
    }
}