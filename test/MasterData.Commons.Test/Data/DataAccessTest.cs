using JJMasterData.Commons.Data;

namespace JJMasterData.Commons.Test.Data;

public class DataAccessTest
{
    private const string TableName = "DataAccessTest";
    private const string ConnectionStringEnvironmentVariable = "JJMASTERDATA_TEST_CONNECTION_STRING";

    private DataAccess DataAccess { get; }

    public DataAccessTest()
    {
        DataAccess = new DataAccess(
            Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable)!,
            DataAccessProvider.SqlServer);
        ConfigureSeedValues();
    }

    public static bool IsDatabaseConfigured =>
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable));
    
    private void ConfigureSeedValues()
    {
        var seedPath = Path.Combine(AppContext.BaseDirectory, "Data", "Seed", "DataAccessTest.sql");
        var sql = File.ReadAllText(seedPath);
        DataAccess.ExecuteBatch(sql);
    }

    [Fact(
        Skip = $"Set {ConnectionStringEnvironmentVariable} to run this database test.",
        SkipUnless = nameof(IsDatabaseConfigured))]
    public async Task GetDataTableTest()
    {
        var dataTable = await DataAccess.GetDataTableAsync(
            $"SELECT * FROM {TableName}",
            cancellationToken: TestContext.Current.CancellationToken);
        
        Assert.True(dataTable.Rows.Count > 0);
    }
}
