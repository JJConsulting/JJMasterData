using System.Collections.Generic;
using System.Linq;
using JJMasterData.Core.DataDictionary.Action;
using JJMasterData.Core.DataDictionary.Action.Form;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary;

public class GridToolbarActionList : FormElementActionList
{
    [JsonIgnore] public InsertAction InsertAction => List.First(a => a.Name == InsertAction.Name) as InsertAction;
    [JsonIgnore] public LegendAction LegendAction => List.First(a => a.Name == LegendAction.Name) as LegendAction;
    [JsonIgnore] public RefreshAction RefreshAction => List.First(a => a.Name == RefreshAction.Name) as RefreshAction;
    [JsonIgnore] public FilterAction FilterAction => List.First(a => a.Name == FilterAction.Name) as FilterAction;
    [JsonIgnore] public ImportAction ImportAction => List.First(a => a.Name == ImportAction.Name) as ImportAction;
    [JsonIgnore] public ExportAction ExportAction => List.First(a => a.Name == ExportAction.Name) as ExportAction;
    [JsonIgnore] public ConfigAction ConfigAction => List.First(a => a.Name == ConfigAction.Name) as ConfigAction;
    [JsonIgnore] public SortAction SortAction => List.First(a => a.Name == SortAction.Name) as SortAction;
    [JsonIgnore] public LogAction LogAction => List.First(a => a.Name == LogAction.Name) as LogAction;

    public GridToolbarActionList()
    {
        List.Add(new InsertAction());
        List.Add(new LegendAction());
        List.Add(new RefreshAction());
        List.Add(new FilterAction());
        List.Add(new ImportAction());
        List.Add(new ExportAction());
        List.Add(new ConfigAction());
        List.Add(new SortAction());
        List.Add(new LogAction());
    }
    
    [JsonConstructor]
    private GridToolbarActionList(IList<BasicAction> list)
    {
        List = list;
    }
}