using JJMasterData.Commons.Data.Csv;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataDictionary.Services;
using JJMasterData.Core.DataManager;
using JJMasterData.WebApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;

namespace JJMasterData.WebApi.Test.Services;

public class MasterApiCsvTests
{
    private static MasterApiService Create(out Mock<IEntityRepository> repository, out FormElement form, string query = "")
    {
        form = new FormElement { Name = "Test" };
        form.ApiOptions.EnableGetAll = true;
        form.ApiOptions.ApplyUserIdOn = "USER_ID";
        form.Fields.Add(new FormElementField { Name = "USER_ID" });
        var dictionaryRepository = new Mock<IDataDictionaryRepository>();
        dictionaryRepository.Setup(x => x.GetFormElementAsync("Test")).ReturnsAsync(form);
        var options = new Mock<IOptionsSnapshot<MasterDataCoreOptions>>();
        options.SetupGet(x => x.Value).Returns(new MasterDataCoreOptions { EnableDataDictionaryCaching = false });
        var dictionary = new DataDictionaryService(dictionaryRepository.Object, Mock.Of<IMemoryCache>(), options.Object);
        repository = new Mock<IEntityRepository>(MockBehavior.Strict);
        var user = new Mock<IMasterDataUser>();
        user.SetupGet(x => x.Id).Returns("current");
        var context = new DefaultHttpContext();
        context.Request.QueryString = new QueryString(query);
        return new MasterApiService(null!, new HttpContextAccessor { HttpContext = context }, user.Object,
            null!, null!, null!, null!, repository.Object, dictionary, null!);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task WriteCsvAsync_ShouldRejectBeforeWriting_WhenUserFilterTargetsAnotherUser(bool query)
    {
        var service = Create(out var repository, out _, query ? "?USER_ID=other" : "");
        using var writer = new StringWriter();
        var parameters = new EntityParameters();
        if (!query) parameters.Filters.Add("USER_ID", "other");
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.WriteCsvAsync("Test", writer, parameters, cancellationToken: TestContext.Current.CancellationToken));
        repository.VerifyNoOtherCalls();
        Assert.Empty(writer.ToString());
    }

    [Fact]
    public async Task WriteCsvAsync_ShouldRejectBeforeWriting_WhenApiIsDisabled()
    {
        var service = Create(out var repository, out var form);
        form.ApiOptions.EnableGetAll = false;
        using var writer = new StringWriter();
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.WriteCsvAsync("Test", writer, cancellationToken: TestContext.Current.CancellationToken));
        repository.VerifyNoOtherCalls();
        Assert.Empty(writer.ToString());
    }

    [Fact]
    public async Task WriteCsvAsync_ShouldPassPaginationUserOptionsAndCancellation_WhenApiIsEnabled()
    {
        var service = Create(out var repository, out var form);
        using var writer = new StringWriter();
        var options = new CsvWriteOptions();
        var token = TestContext.Current.CancellationToken;
        repository.Setup(x => x.WriteCsvAsync(form, writer,
            It.Is<EntityParameters>(p => p.CurrentPage == 2 && p.RecordsPerPage == 500 &&
                (string)p.Filters["USER_ID"]! == "current"), options, token)).ReturnsAsync(500);
        Assert.Equal(500, await service.WriteCsvAsync("Test", writer,
            new EntityParameters { CurrentPage = 2, RecordsPerPage = 500 }, options, token));
        repository.VerifyAll();
    }
}
