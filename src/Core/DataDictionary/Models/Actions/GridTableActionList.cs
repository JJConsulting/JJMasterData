using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public sealed class GridTableActionList : FormElementActionList
{
    private DeleteAction _deleteAction = new();
    private EditAction _editAction = new();
    private ViewAction _viewAction = new();

    [JsonPropertyName("deleteAction")]
    public DeleteAction DeleteAction
    {
        get => (DeleteAction)List.Find(a => a is DeleteAction) ?? _deleteAction;
        set
        {
            _deleteAction = value;
            Set(value);
        }
    }

    [JsonPropertyName("editAction")]
    public EditAction EditAction
    {
        get => (EditAction)List.Find(a => a is EditAction) ?? _editAction;
        set
        {
            _editAction = value;
            Set(value);
        }
    }

    [JsonPropertyName("viewAction")]
    public ViewAction ViewAction
    {
        get => (ViewAction)List.Find(a => a is ViewAction) ?? _viewAction;
        set
        {
            _viewAction = value;
            Set(value);
        }
    }

    public GridTableActionList()
    {
        
    }

    private GridTableActionList(List<BasicAction> list) : base(list)
    {
    }

    public GridTableActionList DeepCopy()
    {
        return new GridTableActionList(List.ConvertAll(a => a.DeepCopy()));
    }
}
