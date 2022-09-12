using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json;

namespace JJMasterData.Commons.Extensions;

public static class DataTableExtensions
{
    public static List<T> ToModelList<T>(this DataTable dataTable)
    {
        var serialized = JsonConvert.SerializeObject(dataTable);

        return JsonConvert.DeserializeObject<List<T>>(serialized);
    }
}
