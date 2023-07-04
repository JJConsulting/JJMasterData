using System.Collections.Generic;
using System.Linq;
using JJMasterData.Core.DataDictionary.Actions.Abstractions;
using JJMasterData.Core.DataDictionary.Actions.GridToolbar;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Actions;

public class GridToolbarActionList : FormElementActionList
{
    public InsertAction InsertAction => List.FirstOrDefault(a => a is InsertAction) as InsertAction;
    public LegendAction LegendAction => List.FirstOrDefault(a => a is LegendAction) as LegendAction;
    public RefreshAction RefreshAction => List.FirstOrDefault(a =>  a is RefreshAction) as RefreshAction;
    public FilterAction FilterAction => List.FirstOrDefault(a => a is FilterAction) as FilterAction;
    public ImportAction ImportAction => List.FirstOrDefault(a=>a is ImportAction) as ImportAction;

    public ExportAction ExportAction => List.FirstOrDefault(a =>a is ExportAction) as ExportAction;
    public ConfigAction ConfigAction => List.FirstOrDefault(a =>a is ConfigAction) as ConfigAction;
    public SortAction SortAction => List.FirstOrDefault(a => a is SortAction) as SortAction;
    public LogAction LogAction => List.FirstOrDefault(a => a is LogAction) as LogAction;

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