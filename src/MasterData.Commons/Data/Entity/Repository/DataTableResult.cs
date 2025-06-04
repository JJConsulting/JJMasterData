using System.Data;

namespace JJMasterData.Commons.Data.Entity.Repository;

public class DataTableResult(DataTable data, int totalOfRecords) : EntityResult<DataTable>(data, totalOfRecords);