using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public sealed class GridTableActionList : FormElementActionList
{
    [JsonPropertyName("deleteAction")]
    public DeleteAction DeleteAction
    {
        get => GetOrAdd<DeleteAction>();
        set => SetOfType(value);
    }

    [JsonPropertyName("editAction")]
    public EditAction EditAction
    {
        get => GetOrAdd<EditAction>();
        set => SetOfType(value);
    }

    [JsonPropertyName("viewAction")]
    public ViewAction ViewAction
    {
        get => GetOrAdd<ViewAction>();
        set => SetOfType(value);
    }

    public GridTableActionList()
    {
        Add(new ViewAction());
        Add(new EditAction());
        Add(new DeleteAction());
    }

    private GridTableActionList(List<BasicAction> list) : base(list)
    {
    }

    public GridTableActionList DeepCopy()
    {
        return new GridTableActionList(List.ConvertAll(a => a.DeepCopy()));
    }
}
