using JJMasterData.Commons.Data.Csv;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager;
using JJMasterData.WebApi.Services;
using Microsoft.AspNetCore.Http;
using Moq;

namespace JJMasterData.WebApi.Test.Services;

public class MasterApiCsvTests
{
    private static (MasterApiService Service, Mock<IEntityRepository> Repository, FormElement Form) Create(string query = "")
    {
        var form = new FormElement { Name = "Test" };
        form.ApiOptions.EnableGetAll = true;
        form.ApiOptions.ApplyUserIdOn = "USER_ID";
        form.Fields.Add(new FormElementField { Name = "USER_ID" });
        var dictionary = new Mock<IDataDictionaryRepository>();
        dictionary.Setup(x => x.GetFormElementAsync("Test")).ReturnsAsync(form);
        var repository = new Mock<IEntityRepository>(MockBehavior.Strict);
        var user = new Mock<IMasterDataUser>();
        user.SetupGet(x => x.Id).Returns("current");
        var context = new DefaultHttpContext();
        context.Request.QueryString = new QueryString(query);
        var service = new MasterApiService(null!, new HttpContextAccessor { HttpContext = context }, user.Object,
            null!, null!, null!, null!, repository.Object, dictionary.Object, null!);
        return (service, repository, form);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RejectsOtherUserBeforeWriting(bool query)
    {
        var (service, repository, _) = Create(query ? "?USER_ID=other" : "");
        using var writer = new StringWriter();
        var parameters = new EntityParameters();
        if (!query) parameters.Filters.Add("USER_ID", "other");
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.WriteCsvAsync("Test", writer, parameters, cancellationToken: TestContext.Current.CancellationToken));
        repository.VerifyNoOtherCalls();
        Assert.Empty(writer.ToString());
    }

    [Fact]
    public async Task RejectsDisabledApiBeforeWriting()
    {
        var (service, repository, form) = Create();
        form.ApiOptions.EnableGetAll = false;
        using var writer = new StringWriter();
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.WriteCsvAsync("Test", writer, cancellationToken: TestContext.Current.CancellationToken));
        repository.VerifyNoOtherCalls();
        Assert.Empty(writer.ToString());
    }

    [Fact]
    public async Task PassesPaginationUserOptionsAndCancellationWithoutMaterializing()
    {
        var (service, repository, form) = Create();
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
