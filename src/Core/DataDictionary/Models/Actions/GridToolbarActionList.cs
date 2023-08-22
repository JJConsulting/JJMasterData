using System.Collections.Generic;
using System.Linq;
using JJMasterData.Core.DataDictionary.Actions.Abstractions;
using JJMasterData.Core.DataDictionary.Actions.GridToolbar;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Actions;

public class GridToolbarActionList : FormElementActionList
{
    public InsertAction InsertAction => (InsertAction)List.First(a => a is InsertAction);
    public LegendAction LegendAction => (LegendAction)List.First(a => a is LegendAction);
    public RefreshAction RefreshAction => (RefreshAction)List.First(a => a is RefreshAction);
    public FilterAction FilterAction => (FilterAction)List.First(a => a is FilterAction);
    public ImportAction ImportAction => (ImportAction)List.First(a => a is ImportAction);
    public ExportAction ExportAction => (ExportAction)List.First(a => a is ExportAction);
    public ConfigAction ConfigAction => (ConfigAction)List.First(a => a is ConfigAction);
    public SortAction SortAction => (SortAction)List.First(a => a is SortAction);
    public LogAction LogAction => (LogAction)List.First(a => a is LogAction);


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