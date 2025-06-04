using System.Collections.Generic;

namespace JJMasterData.Commons.Data.Entity.Repository;

public class ListResult<T>(List<T> data, int totalOfRecords) : EntityResult<List<T>>(data, totalOfRecords)
{
    public int Count => Data.Count;
}