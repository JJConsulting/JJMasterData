using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.Configuration;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataDictionary.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;

namespace JJMasterData.Core.Test.DataDictionary.Services;

public class DataDictionaryServiceTests : IDisposable
{
    private readonly Mock<IDataDictionaryRepository> _repository = new();
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());

    private DataDictionaryService Create(bool enabled = true)
    {
        var options = new Mock<IOptionsSnapshot<MasterDataCoreOptions>>();
        options.SetupGet(x => x.Value).Returns(new MasterDataCoreOptions { EnableDataDictionaryCaching = enabled });
        return new DataDictionaryService(_repository.Object, _cache, options.Object);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ReadsShareCacheAndProtectStoredElements(bool asyncFirst)
    {
        var original = new FormElement { Name = "test", Title = "Original" };
        original.Fields.Add(new FormElementField { Name = "field" });
        _repository.Setup(x => x.GetFormElement("test")).Returns(original);
        _repository.Setup(x => x.GetFormElementAsync("test")).ReturnsAsync(original);
        var service = Create();
        var first = asyncFirst ? await service.GetFormElementAsync("test") : service.GetFormElement("test");
        Assert.NotNull(first);
        first.Title = "Changed";
        first.Fields[0].Name = "changed";
        original.Title = "Changed at source";
        var second = asyncFirst ? service.GetFormElement("test") : await service.GetFormElementAsync("test");
        Assert.NotNull(second);
        Assert.Equal("Original", second.Title);
        Assert.Equal("field", second.Fields[0].Name);
        _repository.Verify(x => x.GetFormElement("test"), asyncFirst ? Times.Never() : Times.Once());
        _repository.Verify(x => x.GetFormElementAsync("test"), asyncFirst ? Times.Once() : Times.Never());
    }

    [Fact]
    public async Task DisabledCacheBypassesExistingEntriesAndDoesNotPopulate()
    {
        _repository.Setup(x => x.GetFormElement("test")).Returns(new FormElement { Name = "test" });
        _repository.Setup(x => x.GetFormElementAsync("test")).ReturnsAsync(new FormElement { Name = "test" });
        _repository.Setup(x => x.GetElementNameListAsync()).ReturnsAsync(["test"]);
        var enabled = Create();
        enabled.GetFormElement("test");
        await enabled.GetElementNameListAsync();
        var disabled = Create(false);
        disabled.GetFormElement("test");
        await disabled.GetFormElementAsync("test");
        await disabled.GetElementNameListAsync();
        _repository.Verify(x => x.GetFormElement("test"), Times.Exactly(2));
        _repository.Verify(x => x.GetFormElementAsync("test"), Times.Once);
        _repository.Verify(x => x.GetElementNameListAsync(), Times.Exactly(2));
        _cache.Clear();
        disabled.GetFormElement("test");
        await disabled.GetElementNameListAsync();
        enabled.GetFormElement("test");
        await enabled.GetElementNameListAsync();
        _repository.Verify(x => x.GetFormElement("test"), Times.Exactly(4));
        _repository.Verify(x => x.GetElementNameListAsync(), Times.Exactly(4));
    }

    [Fact]
    public async Task NameListsAreCopiedOnStorageAndReturn()
    {
        List<string> source = ["test"];
        _repository.Setup(x => x.GetElementNameListAsync()).ReturnsAsync(source);
        var service = Create();
        (await service.GetElementNameListAsync()).Clear();
        source.Clear();
        Assert.Equal(["test"], await service.GetElementNameListAsync());
        _repository.Verify(x => x.GetElementNameListAsync(), Times.Once);
    }

    [Theory]
    [InlineData("sync", false)]
    [InlineData("async", false)]
    [InlineData("batch", false)]
    [InlineData("batch", true)]
    [InlineData("delete", false)]
    public async Task WritesInvalidateElementsNamesAndScripts(string operation, bool fail)
    {
        var element = new FormElement { Name = "test" };
        _repository.Setup(x => x.GetFormElement("test")).Returns(element);
        _repository.Setup(x => x.GetElementNameListAsync()).ReturnsAsync(["test"]);
        _repository.Setup(x => x.InsertOrReplaceAsync(It.IsAny<IEnumerable<FormElement>>()))
            .Returns(fail ? Task.FromException(new InvalidOperationException("Partial write")) : Task.CompletedTask);
        var service = Create();
        service.GetFormElement("test");
        await service.GetElementNameListAsync();
        _cache.Set("test_ReadScript", "read");
        _cache.Set("test_WriteScript", "write");
        var enumerations = 0;
        IEnumerable<FormElement> Elements()
        {
            enumerations++;
            yield return element;
        }
        switch (operation)
        {
            case "sync": service.InsertOrReplace(element); break;
            case "async": await service.InsertOrReplaceAsync(element); break;
            case "batch":
                if (fail)
                    await Assert.ThrowsAsync<InvalidOperationException>(() => service.InsertOrReplaceAsync(Elements()));
                else
                    await service.InsertOrReplaceAsync(Elements());
                Assert.Equal(1, enumerations);
                break;
            case "delete": await service.DeleteAsync("test"); break;
        }
        service.GetFormElement("test");
        await service.GetElementNameListAsync();
        _repository.Verify(x => x.GetFormElement("test"), Times.Exactly(2));
        _repository.Verify(x => x.GetElementNameListAsync(), Times.Exactly(2));
        Assert.False(_cache.TryGetValue((object)"test_ReadScript", out _));
        Assert.False(_cache.TryGetValue((object)"test_WriteScript", out _));
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void SavePreservesProcedureScriptEntries(bool read, bool write)
    {
        _cache.Set("test_ReadScript", "read");
        _cache.Set("test_WriteScript", "write");
        Create().InsertOrReplace(new FormElement { Name = "test", UseReadProcedure = read, UseWriteProcedure = write });
        Assert.Equal(read, _cache.TryGetValue((object)"test_ReadScript", out _));
        Assert.Equal(write, _cache.TryGetValue((object)"test_WriteScript", out _));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ListsForwardFiltersWithoutCaching(bool? apiSync)
    {
        List<FormElement> result = [new FormElement { Name = "test" }];
        _repository.Setup(x => x.GetFormElementList(apiSync)).Returns(result);
        _repository.Setup(x => x.GetFormElementListAsync(apiSync)).ReturnsAsync(result);
        var service = Create();
        for (var i = 0; i < 2; i++)
        {
            Assert.Same(result, service.GetFormElementList(apiSync));
            Assert.Same(result, await service.GetFormElementListAsync(apiSync));
        }
        _repository.Verify(x => x.GetFormElementList(apiSync), Times.Exactly(2));
        _repository.Verify(x => x.GetFormElementListAsync(apiSync), Times.Exactly(2));
    }

    [Fact]
    public async Task MissingElementsPreserveRepositoryResultsAndExceptions()
    {
        _repository.Setup(x => x.GetFormElement("missing")).Returns((FormElement)null!);
        var exception = new JJMasterDataException("missing");
        _repository.Setup(x => x.GetFormElementAsync("missing")).ThrowsAsync(exception);
        var service = Create();
        Assert.Null(service.GetFormElement("missing"));
        Assert.Same(exception, await Assert.ThrowsAsync<JJMasterDataException>(async () => await service.GetFormElementAsync("missing")));
    }

    [Theory]
    [InlineData("sql")]
    [InlineData("files")]
    [InlineData("custom")]
    public void DependencyInjectionUsesConfiguredRepository(string kind)
    {
        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddOptions();
        services.AddDataDictionaryServices();
        if (kind == "sql")
        {
            services.AddSingleton(Mock.Of<JJMasterData.Commons.Data.Entity.Repository.Abstractions.IEntityRepository>());
            services.AddScoped<IDataDictionaryRepository, SqlDataDictionaryRepository>();
        }
        else if (kind == "files")
        {
            services.Configure<FileSystemDataDictionaryOptions>(options => options.FolderPath = Path.GetTempPath());
            services.AddScoped<IDataDictionaryRepository, FileSystemDataDictionaryRepository>();
        }
        else
            services.AddScoped<IDataDictionaryRepository>(_ => _repository.Object);
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<DataDictionaryService>();
        Assert.Same(service, scope.ServiceProvider.GetRequiredService<DataDictionaryService>());
        using var otherScope = provider.CreateScope();
        Assert.NotSame(service, otherScope.ServiceProvider.GetRequiredService<DataDictionaryService>());
    }

    public void Dispose() => _cache.Dispose();
}
