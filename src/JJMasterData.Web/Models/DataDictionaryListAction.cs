using JJMasterData.Core.DataDictionary.Action;
using JJMasterData.Core.WebComponents;

namespace JJMasterData.Web.Models;

public class DataDictionaryListAction
{
    public string? DictionaryName { get; set; }

    public ActionOrigin Context { get; set; }

    public string? FieldName { get; set; }

    public List<BasicAction>? ListAction { get; set; }

}