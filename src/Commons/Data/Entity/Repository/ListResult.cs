using System.Collections.Generic;

namespace JJMasterData.Commons.Data.Entity.Repository;

public class ListResult<T> : EntityResult<IList<T>>
{
    public int Count => Data.Count;
    public ListResult(IList<T> data, int totalOfRecords) : base(data, totalOfRecords)
    {
    }
}