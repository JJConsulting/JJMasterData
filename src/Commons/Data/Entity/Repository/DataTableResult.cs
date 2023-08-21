using System.Data;

namespace JJMasterData.Commons.Data.Entity.Repository;

public class DataTableResult : EntityResult<DataTable>
{
    public DataTableResult(DataTable data, int totalOfRecords) : base(data,totalOfRecords)
    {
    }
}