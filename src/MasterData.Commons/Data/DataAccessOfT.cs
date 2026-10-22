#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JJMasterData.Commons.Data;

public partial class DataAccess
{
    /// <summary>
    /// Reads the first row directly into a model,
    /// or null when there are no rows.
    /// </summary>
    public async Task<T?> GetAsync<T>(
        DataAccessCommand command,
        CancellationToken cancellationToken = default)
        where T : class, IDataAccessModel<T>
    {
        return await ReadAsync<T?>(
            command,
            async (reader, token) =>
            {
                if (await reader.ReadAsync(token).ConfigureAwait(false))
                {
                    var ordinals = T.ResolveOrdinals(reader);
                    return T.Read(reader, ordinals);
                }

                return null;
            },
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Reads rows directly into models.
    /// </summary>
    public async Task<List<T>> GetListAsync<T>(
        DataAccessCommand command,
        CancellationToken cancellationToken = default)
        where T : class, IDataAccessModel<T>
    {
        return await ReadAsync(
            command,
            async (reader, token) =>
            {
                var items = new List<T>();

                if (!await reader.ReadAsync(token).ConfigureAwait(false))
                    return items;

                var ordinals = T.ResolveOrdinals(reader);

                do
                {
                    items.Add(T.Read(reader, ordinals));
                }
                while (await reader.ReadAsync(token).ConfigureAwait(false));

                return items;
            },
            cancellationToken).ConfigureAwait(false);
    }
}
