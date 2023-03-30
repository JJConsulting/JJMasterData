using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Commons.Data;

namespace JJMasterData.Commons.Extensions;

public static class DataAccessExtensions
{
    
    public static T GetModel<T>(this DataAccess dataAccess, DataAccessCommand cmd)
    {
        return dataAccess.GetFields(cmd).ToModel<T>();
    }

    public static async Task<T> GetModelAsync<T>(this DataAccess dataAccess, DataAccessCommand cmd, CancellationToken cancellationToken = default)
    {
        var result = await dataAccess.GetFieldsAsync(cmd, cancellationToken);
        return result.ToModel<T>();
    }
    
    public static IList<T> GetModelList<T>(this DataAccess dataAccess, DataAccessCommand cmd)
    {
        return dataAccess.GetDataTable(cmd).ToModelList<T>();
    }
    
    public static async IAsyncEnumerable<T> GetModelAsyncEnumerable<T>(this DataAccess dataAccess, DataAccessCommand cmd, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var dataTable = await dataAccess.GetDataTableAsync(cmd, cancellationToken);
        var result = dataTable.ToModelList<T>();

        foreach (var model in result)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return model;
        }
    }
}
