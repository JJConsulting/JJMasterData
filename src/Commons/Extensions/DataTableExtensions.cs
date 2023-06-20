#nullable enable

using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json;

namespace JJMasterData.Commons.Extensions;

public static class DataTableExtensions
{
    public static List<T>? ToModelList<T>(this DataTable dataTable, JsonSerializerSettings? jsonSerializerSettings = null)
    {
        var serialized = JsonConvert.SerializeObject(dataTable, jsonSerializerSettings);

        return JsonConvert.DeserializeObject<List<T>>(serialized, jsonSerializerSettings);
    }
}
