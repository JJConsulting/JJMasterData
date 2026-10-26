using JJConsulting.MasterData.Storage.Abstractions;
using JJMasterData.Commons.Background;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Resources;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Exportation;
using JJMasterData.Core.DataManager.Exportation.Abstractions;
using JJMasterData.Core.DataManager.Exportation.Background;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Moq;

namespace JJMasterData.Core.Test.DataManager.Exports;

public sealed class ExportJobHandlerTests
{
    [Theory]
    [InlineData(null, "TenantId ASC,Id ASC", 10_000)]
    [InlineData("Name DESC", "Name DESC,TenantId ASC,Id ASC", 10_000)]
    [InlineData("Id DESC,Name ASC", "Id DESC,Name ASC,TenantId ASC", 10_000)]
    [InlineData("tenantid DESC,id DESC", "tenantid DESC,id DESC", 10_000)]
    [InlineData("Name DESC", "Name DESC,TenantId ASC,Id ASC", 100_000)]
    [InlineData("Name DESC", "Name DESC,TenantId ASC,Id ASC", 7_000)]
    public async Task ExportUsesAllPrimaryKeyFieldsOnEveryPage(string? orderBy, string expectedOrder, int recordsPerPage)
    {
        const int total = 20_001;
        var form = new FormElement { Name = "customers" };
        form.Options.ExportRecordsPerPage = recordsPerPage;
        form.Fields.Add(new FormElementField { Name = "TenantId", IsPk = true, Export = true });
        form.Fields.Add(new FormElementField { Name = "Id", IsPk = true, Export = true });
        form.Fields.Add(new FormElementField { Name = "Name", Export = true });
        form.Options.GridToolbarActions.ExportAction.FileName = "customers";
        var pages = new List<int>();
        var repository = new Mock<IEntityRepository>(MockBehavior.Strict);
        repository.Setup(r => r.GetDictionaryListResultAsync(form, It.IsAny<EntityParameters>(), It.IsAny<bool>()))
            .Returns((Element _, EntityParameters parameters, bool recoverTotal) =>
            {
                Assert.Equal(expectedOrder, parameters.OrderBy.ToQueryParameter());
                Assert.Equal(recordsPerPage, parameters.RecordsPerPage);
                Assert.Equal(parameters.CurrentPage == 1, recoverTotal);
                pages.Add(parameters.CurrentPage);
                var rows = Enumerable.Range(0, total)
                    .Skip((parameters.CurrentPage - 1) * parameters.RecordsPerPage)
                    .Take(parameters.RecordsPerPage)
                    .Select(id => new Dictionary<string, object?>
                    {
                        ["TenantId"] = id / 100,
                        ["Id"] = id % 100,
                        ["Name"] = "Repeated name"
                    }).ToList();
                return Task.FromResult(new DictionaryListResult(rows, recoverTotal ? total : -1));
            });
        var format = new RecordingExportFormat();
        var localizer = new Mock<IStringLocalizer<MasterDataResources>>();
        localizer.Setup(l => l[It.IsAny<string>()])
            .Returns((string name) => new LocalizedString(name, name));
        var handler = new ExportJobHandler(repository.Object, null!, new ExportFormatCatalog([format]),
            Mock.Of<IFileStorage>(), Options.Create(new MasterDataCoreOptions()), localizer.Object,
            Mock.Of<JJMasterData.Core.DataManager.IMasterDataUser>());
        var request = new ExportRequest
        {
            FormElement = form,
            UserId = "test-user",
            FormatId = format.Id,
            ExportAllFields = true,
            OrderBy = orderBy,
            Filters = [],
            OptionsValues = [],
            UserValues = []
        };

        await handler.ExecuteAsync(request, new Progress<BackgroundJobProgress>(), TestContext.Current.CancellationToken);

        Assert.Equal(Enumerable.Range(1, (int)Math.Ceiling(total / (double)recordsPerPage)), pages);
        Assert.Equal(total, format.Keys.Count);
        Assert.Equal(total, format.Keys.Distinct().Count());
        Assert.Equal(Enumerable.Range(0, total).Select(id => (id / 100, id % 100)), format.Keys);
        Assert.Equal(orderBy, request.OrderBy);
    }

    private sealed class RecordingExportOptions : ExportFormatOptions;

    private sealed class RecordingExportFormat : IExportFormat<RecordingExportOptions>
    {
        public string Id => "recording";
        public string DisplayName => "Recording";
        public string FileExtension => "txt";
        public List<(int TenantId, int Id)> Keys { get; } = [];

        public async Task WriteAsync(ExportContext context, RecordingExportOptions options, Stream output,
            CancellationToken cancellationToken)
        {
            await foreach (var row in context.Rows.WithCancellation(cancellationToken))
                Keys.Add(((int)row["TenantId"]!, (int)row["Id"]!));
        }
    }
}
