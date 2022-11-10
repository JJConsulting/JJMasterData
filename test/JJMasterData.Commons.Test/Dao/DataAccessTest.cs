using JJMasterData.Commons.Dao;

namespace JJMasterData.Commons.Test.Dao;

public class DataAccessTest
{
    private const string TableName = "DataAccessTest";

    private IDataAccess DataAccess { get; }

    public DataAccessTest()
    {
        DataAccess = new DataAccess();
        ConfigureSeedValues();
    }
    
    private void ConfigureSeedValues()
    {
        var sql = new StreamReader("Dao/Seed/DataAccessTest.sql").ReadToEnd();
        DataAccess.ExecuteBatch(sql);
    }

    [Theory]
    [InlineData(TableName,true)]
    [InlineData("Foo",false)]
    public async Task TableExistsTest(string table, bool exists)
    {
        Assert.Equal(exists, await DataAccess.TableExistsAsync(table));
    }

    [Fact]
    public async Task GetDataTableTest()
    {
        var dataTable = await DataAccess.GetDataTableAsync($"SELECT * FROM {TableName}");
        
        Assert.True(dataTable.Rows.Count > 0);
    }
}