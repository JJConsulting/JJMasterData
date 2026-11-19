using System.Data;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Providers;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;

namespace JJMasterData.Commons.Test.Data;

public class SqlServerOwnerScopedWriteTests
{
    [Fact]
    public void GetInsertOrReplaceCommand_ShouldUseAtomicOwnerPredicate_WhenWriteIsScoped()
    {
        var owner = new ElementField { Name = "Owner", DataBehavior = FieldBehavior.Real, DataType = FieldType.NVarchar };
        var element = new Element("Documents")
        {
            Fields =
            [
                new ElementField { Name = "Id", IsPk = true, DataBehavior = FieldBehavior.Real, DataType = FieldType.Int },
                new ElementField { Name = "Partition", IsPk = true, EnableOnDelete = false, DataBehavior = FieldBehavior.Real, DataType = FieldType.Int },
                owner
            ]
        };
        var values = new Dictionary<string, object?> { ["Id"] = 7, ["Partition"] = 1, ["Owner"] = "editor" };
        var cache = new Mock<IMemoryCache>();
        cache.Setup(value => value.CreateEntry(It.IsAny<object>())).Returns(Mock.Of<ICacheEntry>());
        var provider = new SqlServerProvider(
            Mock.Of<IConnectionRepository>(), null!, cache.Object, Mock.Of<IOptionsSnapshot<MasterDataCommonsOptions>>());

        var unscoped = provider.GetUpdateCommand(element, values);
        var scoped = provider.GetInsertOrReplaceCommand(element.WithWriteOwner(owner, "editor"), values);

        Assert.DoesNotContain("@__jjMasterDataExpectedOwner", unscoped.Sql);
        Assert.Contains("BEGIN TRANSACTION", scoped.Sql);
        Assert.Contains("WITH (UPDLOCK, HOLDLOCK)", scoped.Sql);
        Assert.Contains("AND CONVERT(nvarchar(max), [Owner]) COLLATE Latin1_General_100_BIN2", scoped.Sql);
        Assert.Contains("IF @@ROWCOUNT = 0 THROW", scoped.Sql);
        var deleteScript = scoped.Sql.Substring(scoped.Sql.IndexOf("DELETE FROM", StringComparison.Ordinal));
        Assert.Contains("AND [Partition] = @Partition", deleteScript);
        var expectedOwner = Assert.Single(scoped.Parameters, parameter => parameter.Name == "@__jjMasterDataExpectedOwner");
        Assert.Equal("editor", expectedOwner.Value);
        Assert.Equal(DbType.String, expectedOwner.Type);
        Assert.Equal(-1, expectedOwner.Size);
    }
}
