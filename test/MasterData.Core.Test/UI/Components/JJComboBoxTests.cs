using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Resources;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.UI.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;

namespace JJMasterData.Core.Test.UI.Components;

public class JJComboBoxTests
{
    [Fact]
    public async Task Combo_Uses_FirstOption_Text_As_Placeholder()
    {
        var comboBox = new JJComboBox(
            new Mock<IHttpContextAccessor>().Object,
            GetDataItemService(),
            GetLocalizer())
        {
            Name = "status",
            Visible = true,
            MultiSelect = true,
            DataItem = new FormElementDataItem
            {
                FirstOption = FirstOptionMode.Choose,
                Items =
                [
                    new DataItemValue { Id = "A", Description = "Active" }
                ]
            }
        };

        var result = await comboBox.GetResultAsync();

        Assert.Contains("class=\"form-control form-select tom-select\"", result.Content);
        Assert.Contains("data-placeholder=\"(Choose)\"", result.Content);
        Assert.Contains("<option value=\"\" disabled=\"disabled\">(Choose)</option>", result.Content);
        Assert.DoesNotContain("title=\"", result.Content);
    }

    private static DataItemService GetDataItemService()
    {
        return new DataItemService(
            Mock.Of<IEntityRepository>(),
            null!,
            null!,
            GetLocalizer(),
            Mock.Of<ILogger<DataItemService>>());
    }

    private static IStringLocalizer<MasterDataResources> GetLocalizer()
    {
        var localizer = new Mock<IStringLocalizer<MasterDataResources>>();
        localizer
            .Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));

        return localizer.Object;
    }
}
