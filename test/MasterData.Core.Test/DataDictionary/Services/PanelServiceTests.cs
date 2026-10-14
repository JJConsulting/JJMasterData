using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Commons.Resources;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataDictionary.Services;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using Microsoft.Extensions.Localization;
using Moq;

namespace JJMasterData.Core.Test.DataDictionary.Services;

public class PanelServiceTests
{
    [Fact]
    public async Task SavePanelAsync_ShouldPersistSelectedFieldOrder_WhenFieldsAreReordered()
    {
        var formElement = new FormElement { Name = "test" };
        var panel = new FormElementPanel { PanelId = 1 };
        formElement.Panels.Add(panel);
        formElement.Fields.Add(new FormElementField { Name = "first", PanelId = 1 });
        formElement.Fields.Add(new FormElementField { Name = "otherPanel", PanelId = 2 });
        formElement.Fields.Add(new FormElementField { Name = "second", PanelId = 1 });
        formElement.Fields.Add(new FormElementField { Name = "newField" });

        var repository = new Mock<IDataDictionaryRepository>();
        repository.Setup(x => x.GetFormElementAsync("test"))
            .ReturnsAsync(formElement);
        repository.Setup(x => x.InsertOrReplaceAsync(It.IsAny<FormElement>()))
            .Returns(Task.CompletedTask);

        var validation = new Mock<IValidationDictionary>();
        validation.SetupGet(x => x.IsValid).Returns(true);
        var expressionProvider = new Mock<ISyncExpressionProvider>();
        expressionProvider.SetupGet(x => x.Prefix).Returns("val:");

        var service = new PanelService(validation.Object,
            [expressionProvider.Object], repository.Object,
            Mock.Of<IStringLocalizer<MasterDataResources>>());

        await service.SavePanelAsync("test", panel, ["second", "newField", "first"]);

        Assert.Equal(["second", "otherPanel", "newField", "first"],
            formElement.Fields.Select(x => x.Name));
        Assert.Equal(["second", "newField", "first"],
            formElement.Fields.Where(x => x.PanelId == 1).Select(x => x.Name));
        repository.Verify(x => x.InsertOrReplaceAsync(formElement), Times.Once);
    }
}
