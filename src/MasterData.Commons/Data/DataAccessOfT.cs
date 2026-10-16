#nullable enable
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace JJMasterData.Commons.Data;

public partial class DataAccess
{
    /// <summary>
    /// Reads the first row directly into a model,
    /// or null when there are no rows.
    /// </summary>
    public async Task<T?> QueryFirstOrDefaultAsync<T>(
        DataAccessCommand command,
        Func<DbDataReader, IDataAccessMapper<T>> mapperFactory,
        CancellationToken cancellationToken = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(mapperFactory);

        return await ReadAsync<T?>(
            command,
            async (reader, token) =>
            {
                if (await reader.ReadAsync(token).ConfigureAwait(false))
                {
                    var mapper = mapperFactory(reader);
                    return mapper.Map(reader);
                }

                return null;
            },
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Reads rows directly into models.
    /// </summary>
    public async Task<List<T>> QueryListAsync<T>(
        DataAccessCommand command,
        Func<DbDataReader, IDataAccessMapper<T>> mapperFactory,
        CancellationToken cancellationToken = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(mapperFactory);

        return await ReadAsync(
            command,
            async (reader, token) =>
            {
                var items = new List<T>();

                if (!await reader.ReadAsync(token).ConfigureAwait(false))
                    return items;

                var mapper = mapperFactory(reader);

                do
                {
                    items.Add(mapper.Map(reader));
                }
                while (await reader.ReadAsync(token).ConfigureAwait(false));

                return items;
            },
            cancellationToken).ConfigureAwait(false);
    }
}
