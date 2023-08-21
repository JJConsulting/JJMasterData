using System.Collections.Generic;

namespace JJMasterData.Commons.Data.Entity.Repository;

public class ListResult<T> : EntityResult<List<T>>
{
    public int Count => Data.Count;
    public ListResult(List<T> data, int totalOfRecords) : base(data, totalOfRecords)
    {
    }
}