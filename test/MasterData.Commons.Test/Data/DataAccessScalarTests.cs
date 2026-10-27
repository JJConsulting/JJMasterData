using System.Data;
using System.Data.Common;
using Moq;
using Moq.Protected;
using JJMasterData.Commons.Data;
using Microsoft.Data.Sqlite;

namespace JJMasterData.Commons.Test.Data;

public class DataAccessScalarTests
{
    private static DataAccess CreateAccess() => new("Data Source=:memory:", SqliteFactory.Instance);
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact]
    public async Task ReadsOnlyFirstRowAndColumnWithTypedConversions()
    {
        var access = CreateAccess();
        Assert.Equal("first", await access.GetStringAsync(new DataAccessCommand("SELECT 'first', 'ignored' UNION ALL SELECT 'second', 'ignored'"), Token));
        Assert.Equal(42, await access.GetIntAsync(new DataAccessCommand("SELECT 42, 99 UNION ALL SELECT 100, 101"), Token));
        Assert.Equal(12.34m, await access.GetDecimalAsync(new DataAccessCommand("SELECT '12.34'"), Token));
        Assert.True(await access.GetBoolAsync(new("SELECT 1, 0 UNION ALL SELECT 0, 1"), Token));
        Assert.False(await access.GetBoolAsync(new("SELECT 0"), Token));
        Assert.Equal(2147483648L, await access.GetLongAsync(new("SELECT 2147483648, 0 UNION ALL SELECT 1, 0"), Token));
        Assert.Equal(new DateTime(2026, 10, 26), await access.GetDateTimeAsync(new("SELECT '2026-10-26', 0 UNION ALL SELECT '2020-01-01', 0"), Token));
        var guid = Guid.NewGuid();
        Assert.Equal(guid, await access.GetGuidAsync(new DataAccessCommand($"SELECT '{guid:D}'"), Token));
    }

    [Theory]
    [InlineData("SELECT NULL")]
    [InlineData("SELECT 1 WHERE 0")]
    public async Task ReturnsNullForNullOrNoRows(string sql)
    {
        var access = CreateAccess();
        Assert.Null(await access.GetStringAsync(new DataAccessCommand(sql), Token));
        Assert.Null(await access.GetIntAsync(new DataAccessCommand(sql), Token));
        Assert.Null(await access.GetDecimalAsync(new DataAccessCommand(sql), Token));
        Assert.Null(await access.GetGuidAsync(new DataAccessCommand(sql), Token));
        Assert.Null(await access.GetBoolAsync(new(sql), Token));
        Assert.Null(await access.GetLongAsync(new(sql), Token));
        Assert.Null(await access.GetDateTimeAsync(new(sql), Token));
    }

    [Fact]
    public async Task RejectsFractionalIntegersOverflowAndInvalidTypesWithQueryContext()
    {
        var access = CreateAccess();
        foreach (var sql in new[] { "SELECT 1.5", "SELECT 2147483648", "SELECT 'invalid'" })
        {
            var exception = await Assert.ThrowsAsync<DataException>(() => access.GetIntAsync(new DataAccessCommand(sql), Token));
            Assert.Equal(sql, exception.Data["DataAccess Query"]);
        }
        await Assert.ThrowsAsync<DataException>(() => access.GetStringAsync(new DataAccessCommand("SELECT 42"), Token));
        await Assert.ThrowsAsync<DataException>(() => access.GetGuidAsync(new DataAccessCommand("SELECT 'invalid'"), Token));
    }

    [Fact]
    public async Task ValidatesNewScalarTypesAndSupportsParameters()
    {
        var access = CreateAccess();
        await Assert.ThrowsAsync<DataException>(() => access.GetBoolAsync(new("SELECT 2"), Token));
        await Assert.ThrowsAsync<DataException>(() => access.GetLongAsync(new("SELECT 1.5"), Token));
        await Assert.ThrowsAsync<DataException>(() => access.GetDateTimeAsync(new("SELECT 'invalid'"), Token));
        var command = new DataAccessCommand("SELECT @value");
        command.AddParameter("@value", 2147483648L, DbType.Int64);
        Assert.Equal(2147483648L, await access.GetLongAsync(command, Token));
    }

    [Theory]
    [InlineData("string")]
    [InlineData("int")]
    [InlineData("decimal")]
    [InlineData("guid")]
    [InlineData("bool")]
    [InlineData("long")]
    [InlineData("datetime")]
    public async Task UsesTypedGettersInsteadOfBoxedValuesForExactProviderTypes(string type)
    {
        var reader = new Mock<DbDataReader>(MockBehavior.Strict);
        reader.Setup(r => r.ReadAsync(Token)).ReturnsAsync(true);
        reader.SetupGet(r => r.FieldCount).Returns(1);
        reader.Setup(r => r.IsDBNullAsync(0, Token)).ReturnsAsync(false);
        reader.Setup(r => r.DisposeAsync()).Returns(ValueTask.CompletedTask);
        var connection = new Mock<DbConnection>();
        var command = new Mock<DbCommand>();
        command.Protected().Setup<DbConnection>("DbConnection").Returns(connection.Object);
        command.Protected().Setup<Task<DbDataReader>>("ExecuteDbDataReaderAsync",
            CommandBehavior.SingleRow | CommandBehavior.SingleResult, Token).ReturnsAsync(reader.Object);
        var factory = new Mock<DbProviderFactory>();
        factory.Setup(f => f.CreateConnection()).Returns(connection.Object);
        factory.Setup(f => f.CreateCommand()).Returns(command.Object);
        var access = new DataAccess("test", factory.Object);
        var query = new DataAccessCommand("SELECT value");

        switch (type)
        {
            case "string":
                reader.Setup(r => r.GetFieldType(0)).Returns(typeof(string));
                reader.Setup(r => r.GetString(0)).Returns("value");
                Assert.Equal("value", await access.GetStringAsync(query, Token));
                break;
            case "int":
                reader.Setup(r => r.GetFieldType(0)).Returns(typeof(int));
                reader.Setup(r => r.GetInt32(0)).Returns(42);
                Assert.Equal(42, await access.GetIntAsync(query, Token));
                break;
            case "decimal":
                reader.Setup(r => r.GetFieldType(0)).Returns(typeof(decimal));
                reader.Setup(r => r.GetDecimal(0)).Returns(12.34m);
                Assert.Equal(12.34m, await access.GetDecimalAsync(query, Token));
                break;
            case "guid":
                var guid = Guid.NewGuid();
                reader.Setup(r => r.GetFieldType(0)).Returns(typeof(Guid));
                reader.Setup(r => r.GetGuid(0)).Returns(guid);
                Assert.Equal(guid, await access.GetGuidAsync(query, Token));
                break;
            case "bool":
                reader.Setup(r => r.GetFieldType(0)).Returns(typeof(bool));
                reader.Setup(r => r.GetBoolean(0)).Returns(true);
                Assert.True(await access.GetBoolAsync(query, Token));
                break;
            case "long":
                reader.Setup(r => r.GetFieldType(0)).Returns(typeof(long));
                reader.Setup(r => r.GetInt64(0)).Returns(2147483648L);
                Assert.Equal(2147483648L, await access.GetLongAsync(query, Token));
                break;
            case "datetime":
                var date = new DateTime(2026, 10, 26);
                reader.Setup(r => r.GetFieldType(0)).Returns(typeof(DateTime));
                reader.Setup(r => r.GetDateTime(0)).Returns(date);
                Assert.Equal(date, await access.GetDateTimeAsync(query, Token));
                break;
        }

        reader.Verify(r => r.GetValue(It.IsAny<int>()), Times.Never);
        reader.Verify(r => r.ReadAsync(Token), Times.Once);
        reader.Verify(r => r.DisposeAsync(), Times.Once);
    }

    [Fact]
    public async Task HonorsCancellation()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => CreateAccess().GetIntAsync(new DataAccessCommand("SELECT 1"), source.Token));
    }
}
