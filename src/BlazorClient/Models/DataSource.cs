using System.Collections.Immutable;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Extensions;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.BlazorClient.Models;

public class DataSource
{
    public ImmutableList<IDictionary<string, dynamic?>> Data { get; }
    public int TotalOfRecords { get; }
    public DataSource(IEnumerable<IDictionary<string, dynamic?>> data, int totalOfRecords)
    {
        Data = data.ToImmutableList();
        TotalOfRecords = totalOfRecords;
    }
    
    public static implicit operator DataSource(EntityResult<IDictionary<string, dynamic?>> result)
    {
        return new DataSource(result.Data, result.TotalOfRecords);
    }

}