#nullable enable

using JJMasterData.Commons.Data.Extensions;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace JJMasterData.Commons.Data;

public partial class DataAccess
{
    public T? GetModel<T>(DataAccessCommand cmd, JsonSerializerSettings? serializerSettings = null)
    {
        var fields = GetFields(cmd);
        return fields == null ? default : fields.ToModel<T>(serializerSettings);
    }

    public async Task<T?> GetModelAsync<T>(DataAccessCommand cmd,
        JsonSerializerSettings? serializerSettings = null,
        CancellationToken cancellationToken = default)
    {
        var fields = await GetFieldsAsync(cmd, cancellationToken);
        return fields == null ? default : fields.ToModel<T>(serializerSettings);
    }

    public IList<T>? GetModelList<T>(DataAccessCommand cmd, JsonSerializerSettings? serializerSettings = null)
    {
        return GetDataTable(cmd).ToModelList<T>(serializerSettings);
    }

    public async IAsyncEnumerable<T>? GetModelAsyncEnumerable<T>(DataAccessCommand cmd, 
        JsonSerializerSettings? serializerSettings = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var dataTable = await GetDataTableAsync(cmd, cancellationToken);
        var result = dataTable.ToModelList<T>(serializerSettings);

        if (result != null)
        {
            foreach (var model in result)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return model;
            }
        }
    }
}