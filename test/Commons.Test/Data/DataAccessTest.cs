using JJMasterData.Commons.Data;

namespace JJMasterData.Commons.Test.Data;

public class DataAccessTest
{
    private const string TableName = "DataAccessTest";

    private DataAccess DataAccess { get; }

    public DataAccessTest()
    {
        DataAccess = new DataAccess("data source=localhost,1433;initial catalog=JJMasterData;user=sa;password=Test@123456;Encrypt=True;Trust Server Certificate=True", DataAccessProvider.SqlServer);
        ConfigureSeedValues();
    }
    
    private void ConfigureSeedValues()
    {
        var sql = new StreamReader("Dao/Seed/DataAccessTest.sql").ReadToEnd();
        DataAccess.ExecuteBatch(sql);
    }

    [Fact]
    public async Task GetDataTableTest()
    {
        var dataTable = await DataAccess.GetDataTableAsync($"SELECT * FROM {TableName}");
        
        Assert.True(dataTable.Rows.Count > 0);
    }
}