using System.Text.Json;
using JJMasterData.Commons.Resources;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataDictionary.Services;
using Microsoft.Extensions.Localization;
using Moq;

namespace JJMasterData.Core.Test.DataDictionary.Services;

public sealed class UIOptionsServiceTests
{
    [Fact]
    public void ExportPageSizeDefaultsForExistingDictionariesAndSurvivesCopyAndSerialization()
    {
        var options = JsonSerializer.Deserialize<FormElementOptions>("{}")!;
        Assert.Equal(10_000, options.ExportRecordsPerPage);

        options.ExportRecordsPerPage = 100_000;
        Assert.Equal(100_000, options.DeepCopy().ExportRecordsPerPage);
        var json = JsonSerializer.Serialize(options);
        Assert.Contains("\"exportRecordsPerPage\":100000", json);
        Assert.Equal(100_000, JsonSerializer.Deserialize<FormElementOptions>(json)!.ExportRecordsPerPage);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    [InlineData(1, true)]
    [InlineData(100_000, true)]
    public async Task EditOptionsValidatesAndPersistsExportPageSize(int pageSize, bool valid)
    {
        var form = new FormElement { Name = "customers" };
        var repository = new Mock<IDataDictionaryRepository>();
        repository.Setup(r => r.GetFormElementAsync(form.Name)).ReturnsAsync(form);
        var errors = new List<string>();
        var validation = new Mock<IValidationDictionary>();
        validation.Setup(v => v.AddError(It.IsAny<string>(), It.IsAny<string>()))
            .Callback((string field, string _) => errors.Add(field));
        validation.SetupGet(v => v.IsValid).Returns(() => errors.Count == 0);
        var localizer = new Mock<IStringLocalizer<MasterDataResources>>();
        localizer.Setup(l => l[It.IsAny<string>()]).Returns((string name) => new LocalizedString(name, name));
        var service = new UIOptionsService(validation.Object, repository.Object, localizer.Object);
        var options = new FormElementOptions { ExportRecordsPerPage = pageSize };

        Assert.Equal(valid, await service.EditOptionsAsync(options, form.Name));

        repository.Verify(r => r.InsertOrReplaceAsync(form), valid ? Times.Once() : Times.Never());
        if (valid)
            Assert.Equal(pageSize, form.Options.ExportRecordsPerPage);
        else
        {
            Assert.Contains(nameof(FormElementOptions.ExportRecordsPerPage), errors);
            Assert.Equal(10_000, form.Options.ExportRecordsPerPage);
        }
    }
}
