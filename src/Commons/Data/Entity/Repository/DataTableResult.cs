using System.Data;

namespace JJMasterData.Commons.Data.Entity.Repository;

public class DataTableResult
{
    public DataTable Data { get; }
    public int TotalOfRecords { get; }
    public DataTableResult(DataTable data, int totalOfRecords)
    {
        Data = data;
        TotalOfRecords = totalOfRecords;
    }
}