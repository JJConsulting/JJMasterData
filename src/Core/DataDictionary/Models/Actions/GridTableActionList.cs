using System.Collections.Generic;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public sealed class GridTableActionList : FormElementActionList
{
    public DeleteAction DeleteAction { get; }
    public EditAction EditAction { get; }
    public ViewAction ViewAction { get; }

    public GridTableActionList()
    {
        DeleteAction = new DeleteAction();
        EditAction = new EditAction();
        ViewAction = new ViewAction();

        List.AddRange([
            DeleteAction,
            EditAction,
            ViewAction
        ]);
    }

    [JsonConstructor]
    private GridTableActionList(List<BasicAction> list)
    {
        List = list;

        DeleteAction = EnsureActionExists<DeleteAction>();
        EditAction = EnsureActionExists<EditAction>();
        ViewAction = EnsureActionExists<ViewAction>();
    }

    public GridTableActionList DeepCopy()
    {
        return new GridTableActionList(List.ConvertAll(a=>a.DeepCopy()));
    }
}