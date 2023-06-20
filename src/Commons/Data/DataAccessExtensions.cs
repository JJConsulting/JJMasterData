#nullable enable
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Extensions;
using Newtonsoft.Json;

namespace JJMasterData.Commons.Data;

public static class DataAccessExtensions
{
    public static T? GetModel<T>(this DataAccess dataAccess, DataAccessCommand cmd,
        JsonSerializerSettings? serializerSettings = null)
    {
        return dataAccess.GetFields(cmd).ToModel<T>(serializerSettings);
    }

    public static async Task<T?> GetModelAsync<T>(this DataAccess dataAccess, DataAccessCommand cmd,
        JsonSerializerSettings? serializerSettings = null, CancellationToken cancellationToken = default)
    {
        var result = await dataAccess.GetFieldsAsync(cmd, cancellationToken);
        return result.ToModel<T>(serializerSettings);
    }

    public static IList<T>? GetModelList<T>(this DataAccess dataAccess, DataAccessCommand cmd,
        JsonSerializerSettings? serializerSettings = null)
    {
        return dataAccess.GetDataTable(cmd).ToModelList<T>(serializerSettings);
    }

    public static async IAsyncEnumerable<T>? GetModelAsyncEnumerable<T>(this DataAccess dataAccess,
        DataAccessCommand cmd, JsonSerializerSettings? serializerSettings = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var dataTable = await dataAccess.GetDataTableAsync(cmd, cancellationToken);
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