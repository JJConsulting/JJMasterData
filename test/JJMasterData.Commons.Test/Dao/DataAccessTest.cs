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
    [InlineData("VarCharColumn","Example",true)]
    [InlineData("VarCharColumn","NotExists",false)]
    [InlineData("IntColumn",0,true)]
    [InlineData("IntColumn",1,false)]
    public async Task ValueExistsTest(string columnName, object value, bool exists)
    {
        switch (value)
        {
            case string str:
                Assert.Equal(exists, await DataAccess.ValueExistsAsync(TableName, columnName, str));
                break;
            case int num:
                Assert.Equal(exists, await DataAccess.ValueExistsAsync(TableName, columnName, num));
                break;
        }
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