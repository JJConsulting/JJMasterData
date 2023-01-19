using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Action;

namespace JJMasterData.Web.Areas.DataDictionary.Models;

public class DataDictionaryListAction
{
    public string? DictionaryName { get; set; }

    public ActionSource Context { get; set; }

    public string? FieldName { get; set; }

    public List<BasicAction>? ListAction { get; set; }

}