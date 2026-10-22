using System;
using System.Data.Common;

namespace JJMasterData.Commons.Data;

/// <summary>Reads a typed model directly from a database result.</summary>
public interface IDataAccessModel<out TSelf> where TSelf : class, IDataAccessModel<TSelf>
{
    /// <summary>Resolves column positions once per query.</summary>
    static abstract int[] ResolveOrdinals(DbDataReader reader);

    /// <summary>Reads the current row using this query's column positions.</summary>
    static abstract TSelf Read(DbDataReader reader, ReadOnlySpan<int> ordinals);
}
