using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Web.Areas.DataDictionary.Models;

public class ApiViewModel : DataDictionaryViewModel
{
    public bool IsSync { get; set; }
    public SyncMode Mode { get; set; }
    public List<ElementField> Fields { get; set; } = null!;
    public ApiSettings ApiSettings { get; set; } = null!;
}