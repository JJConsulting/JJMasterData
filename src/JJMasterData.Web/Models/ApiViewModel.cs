using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Core.DataDictionary.DictionaryDAL;

namespace JJMasterData.Web.Models;

public class ApiViewModel : DataDictionaryViewModel
{
    public bool IsSync { get; set; }
    public SyncMode Mode { get; set; }
    public List<ElementField>? Fields { get; set; }
    public DicApiSettings? ApiSettings { get; set; }
}