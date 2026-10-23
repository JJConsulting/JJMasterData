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
        Assert.Contains("<option value=\"\">(Choose)</option>", result.Content);
        Assert.DoesNotContain("title=\"", result.Content);
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData(",", true)]
    [InlineData("A", false)]
    [InlineData("A,", false)]
    public async Task MultiSelect_All_Is_Selected_Only_When_Filter_Is_Empty(string? postedValue, bool allSelected)
    {
        var context = new DefaultHttpContext();
        if (postedValue != null)
        {
            context.Request.ContentType = "application/x-www-form-urlencoded";
            context.Request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                ["status"] = postedValue
            });
        }

        var comboBox = new JJComboBox(
            new HttpContextAccessor { HttpContext = context }, GetDataItemService(), GetLocalizer())
        {
            Name = "status",
            Visible = true,
            MultiSelect = true,
            DataItem = new FormElementDataItem
            {
                FirstOption = FirstOptionMode.All,
                Items = [new DataItemValue { Id = "A", Description = "Active" }]
            }
        };

        var html = (await comboBox.GetResultAsync()).Content;

        Assert.Contains("data-empty-option=\"true\"", html);
        Assert.Contains(allSelected
            ? "<option value=\"\" selected=\"selected\">(All)</option>"
            : "<option value=\"\">(All)</option>", html);
        if (!allSelected)
            Assert.Contains("<option value=\"A\" selected>Active</option>", html);
    }

    private static DataItemService GetDataItemService()
    {
        return new DataItemService(
            Mock.Of<IEntityRepository>(),
            null!,
            null!,
            GetLocalizer(),
            Mock.Of<ILogger<DataItemService>>(),
            null!);
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
