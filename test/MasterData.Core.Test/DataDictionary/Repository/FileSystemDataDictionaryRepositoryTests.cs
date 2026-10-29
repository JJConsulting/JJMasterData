using System.Text.Json;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataDictionary.Structure;
using Microsoft.Extensions.Options;
using Moq;

namespace JJMasterData.Core.Test.DataDictionary.Repository;

public class FileSystemDataDictionaryRepositoryTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "JJMasterDataTests", Guid.NewGuid().ToString("N"));
    private readonly FileSystemDataDictionaryRepository _repository;

    public FileSystemDataDictionaryRepositoryTests()
    {
        var options = new Mock<IOptionsSnapshot<FileSystemDataDictionaryOptions>>();
        options.SetupGet(x => x.Value).Returns(new FileSystemDataDictionaryOptions { FolderPath = _folder });
        _repository = new FileSystemDataDictionaryRepository(options.Object);
        _repository.CreateStructureIfNotExists();
    }

    [Theory]
    [InlineData(null)]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SyncAndAsyncListsApplySynchronismFilter(bool? filter)
    {
        await _repository.InsertOrReplaceAsync(new FormElement { Name = "Enabled", EnableSynchronism = true });
        _repository.InsertOrReplace(new FormElement { Name = "Disabled", EnableSynchronism = false });

        var expected = filter.HasValue ? new[] { filter.Value ? "Enabled" : "Disabled" } : ["Disabled", "Enabled"];
        Assert.Equal(expected, _repository.GetFormElementList(filter).Select(x => x.Name).Order());
        Assert.Equal(expected, (await _repository.GetFormElementListAsync(filter)).Select(x => x.Name).Order());
    }

    [Fact]
    public async Task NameListsReturnSortedNamesWithoutExtension()
    {
        await _repository.InsertOrReplaceAsync(new FormElement { Name = "Zebra" });
        _repository.InsertOrReplace(new FormElement { Name = "Alpha" });
        File.WriteAllText(Path.Combine(_folder, "ignored.tmp"), "incomplete");

        Assert.Equal(["Alpha", "Zebra"], _repository.GetElementNameList());
        Assert.Equal(["Alpha", "Zebra"], await _repository.GetElementNameListAsync());
        Assert.Equal("Alpha", (await _repository.GetFormElementAsync("Alpha.json")).Name);
    }

    [Theory]
    [InlineData("../outside")]
    [InlineData("..\\outside")]
    [InlineData("/tmp/outside")]
    [InlineData("C:\\outside")]
    [InlineData("child/file")]
    [InlineData("child\\file")]
    [InlineData(".")]
    [InlineData("..")]
    [InlineData("")]
    [InlineData(" ")]
    public async Task AllOperationsRejectInvalidNames(string name)
    {
        Assert.ThrowsAny<ArgumentException>(() => _repository.GetFormElement(name));
        Assert.ThrowsAny<ArgumentException>(() => _repository.Exists(name));
        Assert.ThrowsAny<ArgumentException>(() => _repository.Delete(name));
        Assert.ThrowsAny<ArgumentException>(() => _repository.InsertOrReplace(new FormElement { Name = name }));
        await Assert.ThrowsAnyAsync<ArgumentException>(async () => await _repository.GetFormElementAsync(name));
        await Assert.ThrowsAnyAsync<ArgumentException>(() => _repository.InsertOrReplaceAsync(new FormElement { Name = name }));
        await Assert.ThrowsAnyAsync<ArgumentException>(() => _repository.DeleteAsync(name));
        await Assert.ThrowsAnyAsync<ArgumentException>(() => _repository.ExistsAsync(name));
        Assert.Empty(Directory.GetFileSystemEntries(_folder));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ReplacementPreservesAnOpenReaderAndPublishesCompleteJson(bool asynchronous)
    {
        _repository.InsertOrReplace(new FormElement { Name = "Example", Info = "Original" });
        var path = Path.Combine(_folder, "Example.json");
        using var reader = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);

        var replacement = new FormElement { Name = "Example", Info = new string('x', 100_000) };
        if (asynchronous)
            await _repository.InsertOrReplaceAsync(replacement);
        else
            _repository.InsertOrReplace(replacement);

        Assert.Equal("Original", JsonSerializer.Deserialize<FormElement>(reader)!.Info);
        Assert.Equal(replacement.Info, _repository.GetFormElement("Example")!.Info);
        Assert.Equal(replacement.Info, (await _repository.GetFormElementAsync("Example")).Info);
        Assert.Equal([path], Directory.GetFiles(_folder));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task FailedReplacementRemovesTemporaryFile(bool asynchronous)
    {
        var destination = Path.Combine(_folder, "Example.json");
        Directory.CreateDirectory(destination);
        var element = new FormElement { Name = "Example" };

        if (asynchronous)
            await Assert.ThrowsAnyAsync<IOException>(() => _repository.InsertOrReplaceAsync(element));
        else
            Assert.ThrowsAny<IOException>(() => _repository.InsertOrReplace(element));

        Assert.Empty(Directory.GetFiles(_folder));
        Assert.True(Directory.Exists(destination));
    }

    [Fact]
    public async Task InfoListsIgnoreEmptyTableFilterAndApplyNameAndTableFilters()
    {
        await _repository.InsertOrReplaceAsync(new[]
        {
            new FormElement { Name = "Customer", TableName = "Customers" },
            new FormElement { Name = "Order", TableName = "Orders" }
        });
        await AssertInfoList(new DataDictionaryFilter { ContainsTableName = [] }, null, ["Customer", "Order"]);
        await AssertInfoList(new DataDictionaryFilter { ContainsTableName = ["CUSTOM", "missing"], Name = "omer" }, null, ["Customer"]);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DateFilterSkipsInvalidJsonBeforeDeserializing(bool excludeOld)
    {
        _repository.InsertOrReplace(new FormElement { Name = "Valid" });
        var invalid = Path.Combine(_folder, "Invalid.json");
        File.WriteAllText(invalid, "invalid json");
        var now = DateTime.Now;
        File.SetLastWriteTime(invalid, excludeOld ? now.AddDays(-2) : now.AddDays(2));
        var filter = excludeOld
            ? new DataDictionaryFilter { LastModifiedFrom = now.AddDays(-1) }
            : new DataDictionaryFilter { LastModifiedTo = now.AddDays(1) };

        await AssertInfoList(filter, null, ["Valid"]);
    }

    [Fact]
    public async Task InfoListsSupportMultipleSortFieldsAndPagination()
    {
        await _repository.InsertOrReplaceAsync(new[]
        {
            new FormElement { Name = "Alpha", TableName = "Same" },
            new FormElement { Name = "Beta", TableName = "Same" },
            new FormElement { Name = "Gamma", TableName = "Other" }
        });
        var order = new OrderByData().AddOrReplace(DataDictionaryStructure.TableName, OrderByDirection.Asc)
            .AddOrReplace(DataDictionaryStructure.Name, OrderByDirection.Desc);
        await AssertInfoList(new DataDictionaryFilter(), order, ["Gamma", "Beta", "Alpha"]);

        var page = await _repository.GetFormElementInfoListAsync(new DataDictionaryFilter(), order, 1, 2);
        Assert.Equal(["Beta"], page.Data.Select(x => x.Name));
        Assert.Equal(3, page.TotalOfRecords);
        var total = -1;
        Assert.Equal(["Beta"], _repository.GetMetadataInfoList(null, order, 1, 2, ref total).Select(x => x.Name));
        Assert.Equal(3, total);
    }

    [Fact]
    public async Task SynchronismColumnCanBeSorted()
    {
        _repository.InsertOrReplace(new FormElement { Name = "Alpha", EnableSynchronism = true });
        _repository.InsertOrReplace(new FormElement { Name = "Beta", EnableSynchronism = false });
        var order = new OrderByData().AddOrReplace(DataDictionaryStructure.EnableSynchronism, OrderByDirection.Asc);
        await AssertInfoList(new DataDictionaryFilter(), order, ["Beta", "Alpha"]);
    }

    [Fact]
    public async Task DefaultAndEmptyOrderingAreStableByName()
    {
        _repository.InsertOrReplace(new FormElement { Name = "Zebra" });
        _repository.InsertOrReplace(new FormElement { Name = "Alpha" });
        await AssertInfoList(new DataDictionaryFilter(), null, ["Alpha", "Zebra"]);
        await AssertInfoList(new DataDictionaryFilter(), new OrderByData(), ["Alpha", "Zebra"]);
    }

    [Fact]
    public async Task MissingFolderReturnsEmptyListsAndResetsTotal()
    {
        Directory.Delete(_folder);
        Assert.Empty(_repository.GetFormElementList());
        Assert.Empty(await _repository.GetFormElementListAsync());
        Assert.Empty(await _repository.GetElementNameListAsync());
        var total = 123;
        Assert.Empty(_repository.GetMetadataInfoList(null, null, 10, 1, ref total));
        Assert.Equal(0, total);
        var result = await _repository.GetFormElementInfoListAsync(new DataDictionaryFilter(), null, 10, 1);
        Assert.Empty(result.Data);
        Assert.Equal(0, result.TotalOfRecords);
    }

    private async Task AssertInfoList(DataDictionaryFilter filter, OrderByData? order, string[] names)
    {
        var total = -1;
        Assert.Equal(names, _repository.GetMetadataInfoList(filter, order, 100, 1, ref total).Select(x => x.Name));
        Assert.Equal(names.Length, total);
        var result = await _repository.GetFormElementInfoListAsync(filter, order, 100, 1);
        Assert.Equal(names, result.Data.Select(x => x.Name));
        Assert.Equal(names.Length, result.TotalOfRecords);
    }

    public void Dispose()
    {
        if (Directory.Exists(_folder))
            Directory.Delete(_folder, true);
    }
}
