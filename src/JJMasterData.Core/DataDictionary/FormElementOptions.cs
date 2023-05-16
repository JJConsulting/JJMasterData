#nullable enable

namespace JJMasterData.Core.DataDictionary;

public class FormElementOptions
{
    public GridUI Grid { get; set; }
    
    public FormUI Form { get; set; }
    
    public GridToolBarActions ToolBarActions { get; set; }

    public GridActions GridActions { get; set; }

    public FormElementOptions()
    {
        Grid = new GridUI();
        Form = new FormUI();
        ToolBarActions = new GridToolBarActions();
        GridActions = new GridActions();
    }
}