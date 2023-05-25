using System.Collections.Generic;
using System.Linq;
using JJMasterData.Core.DataDictionary.Action;
using JJMasterData.Core.DataDictionary.Action.Form;

namespace JJMasterData.Core.DataDictionary;

public class GridToolbarActionList : FormElementActionList
{
    public InsertAction InsertAction => List.First(a => a.Name == InsertAction.Name) as InsertAction;

    public LegendAction LegendAction => List.First(a => a.Name == LegendAction.Name) as LegendAction;

    public RefreshAction RefreshAction => List.First(a => a.Name == RefreshAction.Name) as RefreshAction;

    public FilterAction FilterAction => List.First(a => a.Name == FilterAction.Name) as FilterAction;

    public ImportAction ImportAction => List.First(a => a.Name == ImportAction.Name) as ImportAction;

    public ExportAction ExportAction => List.First(a => a.Name == ExportAction.Name) as ExportAction;

    public ConfigAction ConfigAction => List.First(a => a.Name == ConfigAction.Name) as ConfigAction;

    public SortAction SortAction => List.First(a => a.Name == SortAction.Name) as SortAction;

    public LogAction LogAction => List.First(a => a.Name == LogAction.Name) as LogAction;


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
}