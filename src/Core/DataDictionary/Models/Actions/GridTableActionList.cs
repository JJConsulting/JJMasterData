using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public sealed class GridTableActionList : FormElementActionList
{
    [JsonPropertyName("deleteAction")]
    public DeleteAction DeleteAction
    {
        get => GetOrAdd<DeleteAction>();
        set => Set(value);
    }

    [JsonPropertyName("editAction")]
    public EditAction EditAction
    {
        get => GetOrAdd<EditAction>();
        set => Set(value);
    }

    [JsonPropertyName("viewAction")]
    public ViewAction ViewAction
    {
        get => GetOrAdd<ViewAction>();
        set => Set(value);
    }

    public GridTableActionList()
    {
        Set(new ViewAction());
        Set(new EditAction());
        Set(new DeleteAction());
    }

    private GridTableActionList(List<BasicAction> list) : base(list)
    {
    }

    public GridTableActionList DeepCopy()
    {
        return new GridTableActionList(List.ConvertAll(a => a.DeepCopy()));
    }
}
