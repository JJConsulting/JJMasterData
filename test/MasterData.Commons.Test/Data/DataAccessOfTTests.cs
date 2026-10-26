using System.Data;
using System.Data.Common;
using System.Text.Json.Serialization;
using JJMasterData.Commons.Data;
using Microsoft.Data.Sqlite;
using Moq;
using Moq.Protected;

namespace JJMasterData.Commons.Test.Data;

public enum QueryStatus : short { Active = 1 }

[DataAccessModel]
public partial class QueryCustomer
{
    [DataAccessColumn("customer_id")] public int Id { get; set; }
    public string Name { get; set; } = "initial";
    public string? Note { get; set; } = "initial";
    public int? Optional { get; set; } = 42;
    public decimal Balance { get; set; }
    public QueryStatus Status { get; set; }
    public QueryStatus? OptionalStatus { get; set; } = QueryStatus.Active;
    public Guid Guid { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTimeOffset? Offset { get; set; }
    public TimeSpan? Duration { get; set; }
    public bool Active { get; set; }
    public char Initial { get; set; }
}

[DataAccessModel]
public partial class QueryOrder
{
    public QueryOrder() => Context = "constructor context";

    [JsonPropertyName("order_id")]
    public int Id { get; set; }
    public QueryOrderDetails Details { get; set; } = null!;
    public QueryCustomer? Customer { get; set; }
    [DataAccessIgnore] public required string Context { get; init; } = "context";
    [DataAccessIgnore] public object State { get; set; } = "initial";
    [DataAccessIgnore] public string Summary => $"Order {Id}";
}

[DataAccessModel]
public partial class QueryOrderDetails
{
    [DataAccessColumn("description"), JsonPropertyName("wrong_description")]
    public string Description { get; set; } = "initial";
    public QueryOrderAddress Address { get; set; } = null!;
}

[DataAccessModel]
public partial class QueryOrderAddress
{
    [JsonPropertyName("city_name")]
    public string? City { get; set; } = "initial";
}

public class TypedDataAccessTests
{
    private static DataAccess CreateAccess() => new("Data Source=:memory:", SqliteFactory.Instance);
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact]
    public void GeneratedMapperUsesCentralizedTypedReaderWithoutBoxing()
    {
        var reader = new Mock<DbDataReader>(MockBehavior.Strict);
        reader.SetupGet(r => r.FieldCount).Returns(1);
        reader.Setup(r => r.GetName(0)).Returns("customer_id");
        reader.Setup(r => r.IsDBNull(0)).Returns(false);
        reader.Setup(r => r.GetFieldType(0)).Returns(typeof(int));
        reader.Setup(r => r.GetInt32(0)).Returns(42);

        var customer = ReadModel<QueryCustomer>(reader.Object);

        static T ReadModel<T>(DbDataReader dataReader) where T : class, IDataAccessModel<T>
            => T.Read(dataReader, T.ResolveOrdinals(dataReader));

        Assert.Equal(42, customer.Id);
        reader.Verify(r => r.GetInt32(0), Times.Once);
        reader.Verify(r => r.GetValue(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task MapsNestedModelsFromEachCurrentRowAndHonorsMappingAttributes()
    {
        var orders = await CreateAccess().GetListAsync<QueryOrder>(new("""
            SELECT 1 AS order_id, 7 AS customer_id, 'Jane' AS Name,
                   'First' AS description, 'ignored' AS wrong_description,
                   'London' AS city_name, 'ignored' AS State, 'ignored' AS Context
            UNION ALL SELECT 2, 8, 'John', 'Second', 'ignored', NULL, 'ignored', 'ignored'
            """), Token);

        Assert.Equal(2, orders.Count);
        Assert.Equal(1, orders[0].Id);
        Assert.Equal(2, orders[1].Id);
        Assert.Equal(7, orders[0].Customer!.Id);
        Assert.Equal(8, orders[1].Customer!.Id);
        Assert.Equal("Jane", orders[0].Customer!.Name);
        Assert.Equal("John", orders[1].Customer!.Name);
        Assert.Equal("First", orders[0].Details.Description);
        Assert.Equal("Second", orders[1].Details.Description);
        Assert.Equal("London", orders[0].Details.Address.City);
        Assert.Null(orders[1].Details.Address.City);
        Assert.NotSame(orders[0].Customer, orders[1].Customer);
        Assert.Equal("initial", orders[0].State);
        Assert.Equal("constructor context", orders[0].Context);
        Assert.Equal("Order 1", orders[0].Summary);

        var missing = await CreateAccess().GetAsync<QueryOrder>(new("SELECT 3 AS order_id"), Token);
        Assert.Equal("initial", missing!.Details.Description);
        Assert.Equal("initial", missing.Details.Address.City);
        Assert.Equal("initial", missing.Customer!.Name);
    }

    [Fact]
    public async Task MapsSqliteDirectlyWithParametersAliasesNullsExtraAndMissingColumns()
    {
        var access = CreateAccess();
        var command = new DataAccessCommand("""
            SELECT @id AS CUSTOMER_ID, 'Jane' AS NAME, NULL AS Note, NULL AS Optional,
                   12.5 AS Balance, 1 AS Status, '2026-10-13 10:15:00' AS CreatedAt,
                   '2026-10-13T10:15:00-03:00' AS Offset, '01:02:03' AS Duration,
                   'e48b83a0-2ad6-48b7-b9ae-08c4ebf5b827' AS Guid, 1 AS Active,
                   'J' AS Initial, 'ignored' AS Extra
            UNION ALL SELECT 8, 'John', 'note', 3, 20.0, 1, NULL, NULL, NULL,
                   'e48b83a0-2ad6-48b7-b9ae-08c4ebf5b827', 0, 'J', 'ignored'
            """);
        command.AddParameter("@id", 7, DbType.Int32);
        var list = await access.GetListAsync<QueryCustomer>(command, Token);
        Assert.Equal(2, list.Count);
        var first = list[0];
        Assert.Equal(7, first.Id);
        Assert.Equal("Jane", first.Name);
        Assert.Null(first.Note);
        Assert.Null(first.Optional);
        Assert.Equal(12.5m, first.Balance);
        Assert.Equal(QueryStatus.Active, first.Status);
        var nullableEnum = await access.GetAsync<QueryCustomer>(new("SELECT NULL AS OptionalStatus"), Token);
        Assert.Null(nullableEnum!.OptionalStatus);
        var numericEnum = await access.GetAsync<QueryCustomer>(new("SELECT 1 AS OptionalStatus"), Token);
        Assert.Equal(QueryStatus.Active, numericEnum!.OptionalStatus);
        Assert.Equal(new DateTime(2026, 10, 13, 10, 15, 0), first.CreatedAt);
        Assert.Equal(TimeSpan.FromHours(-3), first.Offset!.Value.Offset);
        Assert.Equal(new TimeSpan(1, 2, 3), first.Duration);
        Assert.NotEqual(Guid.Empty, first.Guid);
        Assert.True(first.Active);
        Assert.Equal('J', first.Initial);
        Assert.False(list[1].Active);
        Assert.Null(list[1].CreatedAt);
        var single = await access.GetAsync<QueryCustomer>(command, Token);
        Assert.Equal(7, single!.Id);
        var missing = await access.GetAsync<QueryCustomer>(new("SELECT 9 AS customer_id"), Token);
        Assert.Equal("initial", missing!.Name);
        Assert.Equal(42, missing.Optional);
    }

    [Fact]
    public async Task HandlesEmptyResultsAndSchemaChangesOnReusedAccess()
    {
        var access = CreateAccess();
        Assert.Null(await access.GetAsync<QueryCustomer>(new("SELECT 1 AS customer_id WHERE 0"), Token));
        Assert.Empty(await access.GetListAsync<QueryCustomer>(new("SELECT 1 AS customer_id WHERE 0"), Token));
        Assert.Equal(1, (await access.GetAsync<QueryCustomer>(new("SELECT 'one' AS Name, 1 AS customer_id"), Token))!.Id);
        var second = await access.GetAsync<QueryCustomer>(new("SELECT 2 AS customer_id, 'two' AS Name"), Token);
        Assert.Equal(2, second!.Id);
        Assert.Equal("two", second.Name);
    }

    [Fact]
    public async Task ChecksRuntimeTypesPerRowEvenWhenSqliteSchemaIsUnchanged()
    {
        var rows = await CreateAccess().GetListAsync<QueryCustomer>(new("""
            SELECT 1 AS customer_id, NULL AS Note
            UNION ALL SELECT 2.0, 'second'
            """), Token);
        Assert.Equal(1, rows[0].Id);
        Assert.Null(rows[0].Note);
        Assert.Equal(2, rows[1].Id);
        Assert.Equal("second", rows[1].Note);
        await Assert.ThrowsAsync<DataException>(() => CreateAccess().GetListAsync<QueryCustomer>(new("""
            SELECT 1 AS customer_id UNION ALL SELECT 2.5
            """), Token));
    }

    [Theory]
    [InlineData("SELECT NULL AS customer_id", "customer_id")]
    [InlineData("SELECT NULL AS Name", "Name")]
    [InlineData("SELECT 2147483648 AS customer_id", "customer_id")]
    [InlineData("SELECT 1.5 AS customer_id", "customer_id")]
    [InlineData("SELECT '12' AS customer_id", "customer_id")]
    [InlineData("SELECT 3 AS Active", "Active")]
    [InlineData("SELECT 1 AS customer_id, 2 AS CUSTOMER_ID", "customer_id")]
    public async Task RejectsUnsafeConversionsAndAmbiguousColumns(string sql, string column)
    {
        var exception = await Assert.ThrowsAsync<DataException>(() => CreateAccess().GetAsync<QueryCustomer>(new(sql), Token));
        Assert.Contains(column, exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(sql, exception.Data["DataAccess Query"]);
    }

    [Fact]
    public async Task PropagatesCancellationAndAccessRemainsReusable()
    {
        var access = CreateAccess();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => access.GetListAsync<QueryCustomer>(new("SELECT 1"), cancellation.Token));
        Assert.NotNull(await access.GetAsync<QueryCustomer>(new("SELECT 1 AS customer_id"), Token));
    }

    [Theory]
    [InlineData("success")]
    [InlineData("map")]
    [InlineData("read")]
    [InlineData("execute")]
    [InlineData("open")]
    [InlineData("cancel")]
    public async Task DisposesResourcesOnAllPathsAndPropagatesToken(string outcome)
    {
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var connection = new Mock<DbConnection>();
        var command = new Mock<DbCommand>();
        command.SetupAllProperties();
        command.Protected().Setup<DbConnection>("DbConnection").Returns(connection.Object);
        var reader = new Mock<DbDataReader>();
        reader.SetupGet(r => r.FieldCount).Returns(1);
        reader.Setup(r => r.GetName(0)).Returns("customer_id");
        reader.Setup(r => r.GetFieldType(0)).Returns(outcome == "map" ? typeof(string) : typeof(int));
        reader.Setup(r => r.GetInt32(0)).Returns(1);
        reader.Setup(r => r.GetValue(0)).Returns("bad");
        reader.SetupSequence(r => r.ReadAsync(token)).ReturnsAsync(true).ReturnsAsync(true).ReturnsAsync(false);
        var factory = new Mock<DbProviderFactory>();
        factory.Setup(f => f.CreateConnection()).Returns(connection.Object);
        factory.Setup(f => f.CreateCommand()).Returns(command.Object);
        connection.Setup(c => c.OpenAsync(token)).Returns(Task.CompletedTask);
        command.Protected().Setup<Task<DbDataReader>>("ExecuteDbDataReaderAsync", ItExpr.IsAny<CommandBehavior>(), token).ReturnsAsync(reader.Object);
        if (outcome == "read") reader.Setup(r => r.ReadAsync(token)).ThrowsAsync(new InvalidOperationException("read failure"));
        if (outcome == "execute") command.Protected().Setup<Task<DbDataReader>>("ExecuteDbDataReaderAsync", ItExpr.IsAny<CommandBehavior>(), token).ThrowsAsync(new InvalidOperationException("execute failure"));
        if (outcome == "open") connection.Setup(c => c.OpenAsync(token)).ThrowsAsync(new InvalidOperationException("open failure"));
        if (outcome == "cancel") reader.Setup(r => r.ReadAsync(token)).ThrowsAsync(new OperationCanceledException(token));
        var query = new DataAccessCommand("SELECT customer_id") { TimeoutSeconds = 17 };
        var access = new DataAccess("test", factory.Object);
        if (outcome == "success")
        {
            Assert.Equal(2, (await access.GetListAsync<QueryCustomer>(query, token)).Count);
            // One schema pass per result, and one value fetch per mapped cell.
            reader.Verify(r => r.GetName(0), Times.Once);
            reader.Verify(r => r.GetInt32(0), Times.Exactly(2));
            reader.Verify(r => r.GetValue(0), Times.Never);
            reader.Verify(r => r.IsDBNull(0), Times.Exactly(2));
        }
        else
        {
            var error = await Assert.ThrowsAnyAsync<Exception>(() => access.GetListAsync<QueryCustomer>(query, token));
            Assert.Equal(query.Sql, error.Data["DataAccess Query"]);
            if (outcome == "cancel") Assert.IsType<OperationCanceledException>(error);
        }
        Assert.Equal(17, command.Object.CommandTimeout);
        connection.Verify(c => c.OpenAsync(token), Times.Once);
        connection.Verify(c => c.DisposeAsync(), Times.Once);
        command.Verify(c => c.DisposeAsync(), Times.Once);
        reader.Verify(r => r.DisposeAsync(), outcome is "open" or "execute" ? Times.Never : Times.Once);
        if (outcome is not "open" and not "execute") reader.Verify(r => r.ReadAsync(token), Times.AtLeastOnce);
    }
    [Theory]
    [InlineData("typed")]
    [InlineData("typed-list")]
    [InlineData("dictionary-list-async")]
    [InlineData("dictionary-list")]
    [InlineData("dictionary-async")]
    [InlineData("dictionary")]
    [InlineData("hashtable-async")]
    [InlineData("hashtable")]
    [InlineData("datatable-async")]
    [InlineData("dataset-async")]
    [InlineData("datatable")]
    [InlineData("dataset")]
    public async Task CopiesOutputParametersAfterReaderDisposal(string method)
    {
        var connection = new Mock<DbConnection>();
        connection.Setup(c => c.OpenAsync(Token)).Returns(Task.CompletedTask);
        var command = new Mock<DbCommand>();
        command.SetupAllProperties();
        command.Protected().Setup<DbConnection>("DbConnection").Returns(connection.Object);
        await using var parameterOwner = new Microsoft.Data.SqlClient.SqlCommand();
        command.Protected().Setup<DbParameterCollection>("DbParameterCollection").Returns(parameterOwner.Parameters);
        var reader = new Mock<DbDataReader>();
        reader.Setup(r => r.ReadAsync(Token)).ReturnsAsync(false);
        void CompleteOutputs()
        {
            parameterOwner.Parameters["@out"].Value = 123;
            parameterOwner.Parameters["@total"].Value = 456;
        }
        reader.Setup(r => r.Close()).Callback(CompleteOutputs);
        reader.Setup(r => r.DisposeAsync()).Callback(CompleteOutputs).Returns(ValueTask.CompletedTask);
        reader.Protected().Setup("Dispose", ItExpr.IsAny<bool>()).Callback<bool>(_ => CompleteOutputs());
        command.Protected().Setup<DbDataReader>("ExecuteDbDataReader", ItExpr.IsAny<CommandBehavior>()).Returns(reader.Object);
        command.Protected().Setup<Task<DbDataReader>>("ExecuteDbDataReaderAsync", ItExpr.IsAny<CommandBehavior>(), Token).ReturnsAsync(reader.Object);
        var factory = new Mock<DbProviderFactory>();
        factory.Setup(f => f.CreateConnection()).Returns(connection.Object);
        factory.Setup(f => f.CreateCommand()).Returns(command.Object);
        factory.Setup(f => f.CreateParameter()).Returns(() => new Microsoft.Data.SqlClient.SqlParameter());
        var output = new DataAccessParameter("@out", DBNull.Value, DbType.Int32, ParameterDirection.Output);
        var total = new DataAccessParameter("@total", 1, DbType.Int32, ParameterDirection.InputOutput);
        var query = new DataAccessCommand("EXEC test", [output, total], CommandType.StoredProcedure);
        factory.Setup(f => f.CreateDataAdapter()).Returns(() => new Mock<DbDataAdapter> { CallBase = true }.Object);
        var access = new DataAccess("test", factory.Object);
        switch (method)
        {
            case "typed": Assert.Null(await access.GetAsync<QueryCustomer>(query, Token)); break;
            case "typed-list": Assert.Empty(await access.GetListAsync<QueryCustomer>(query, Token)); break;
            case "dictionary-list-async": Assert.Empty(await access.GetDictionaryListAsync(query, Token)); break;
            case "dictionary-list": Assert.Empty(access.GetDictionaryList(query)); break;
            case "dictionary-async": Assert.Empty(await access.GetDictionaryAsync(query, Token)); break;
            case "dictionary": Assert.Null(access.GetDictionary(query)); break;
            case "hashtable-async": Assert.Null(await access.GetHashtableAsync(query, Token)); break;
            case "hashtable": Assert.Null(access.GetHashtable(query)); break;
            case "datatable-async": await access.GetDataTableAsync(query, Token); break;
            case "dataset-async": await access.GetDataSetAsync(query, Token); break;
            case "datatable": access.GetDataTable(query); break;
            case "dataset": access.GetDataSet(query); break;
            default: throw new ArgumentOutOfRangeException(nameof(method));
        }
        Assert.Equal(123, output.Value);
        Assert.Equal(456, total.Value);
        Assert.Equal(CommandType.StoredProcedure, command.Object.CommandType);
    }

    [Fact]
    public async Task DisposesCommandWhenConfigurationFails()
    {
        var command = new Mock<DbCommand>();
        var factory = new Mock<DbProviderFactory>();
        factory.Setup(f => f.CreateCommand()).Returns(command.Object);
        var access = new DataAccess("test", factory.Object);
        await Assert.ThrowsAsync<JJMasterData.Commons.Exceptions.DataAccessException>(() => access.GetListAsync<QueryCustomer>(new(""), Token));
        command.Protected().Verify("Dispose", Times.Once(), ItExpr.Is<bool>(disposing => disposing));
        factory.Verify(f => f.CreateConnection(), Times.Never);
    }

}
