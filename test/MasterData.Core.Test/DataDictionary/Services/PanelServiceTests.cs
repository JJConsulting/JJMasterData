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
    [Theory]
    [InlineData(1, 3, null, false)] // Descendant would create a cycle.
    [InlineData(2, 2, null, false)]
    [InlineData(2, 99, null, false)]
    [InlineData(99, null, null, false)]
    [InlineData(2, null, 3, false)] // Insertion target has another parent.
    [InlineData(2, null, 1, true)]
    [InlineData(2, 4, null, true)]
    public async Task MovePanelAsync_ValidatesHierarchyAndPreservesFields(int panelId, int? parentId,
        int? beforePanelId, bool expected)
    {
        var form = new FormElement { Name = "test" };
        form.Panels.AddRange([
            new FormElementPanel { PanelId = 1 },
            new FormElementPanel { PanelId = 2, ParentId = 1 },
            new FormElementPanel { PanelId = 3, ParentId = 2 },
            new FormElementPanel { PanelId = 4 }
        ]);
        var field = new FormElementField { Name = "field", PanelId = 2 };
        form.Fields.Add(field);
        var repository = new Mock<IDataDictionaryRepository>();
        repository.Setup(x => x.GetFormElementAsync("test")).ReturnsAsync(form);
        repository.Setup(x => x.InsertOrReplaceAsync(form)).Returns(Task.CompletedTask);
        var service = new PanelService(Mock.Of<IValidationDictionary>(), [], repository.Object,
            Mock.Of<IStringLocalizer<MasterDataResources>>());

        Assert.Equal(expected, await service.MovePanelAsync("test", panelId, parentId, beforePanelId));

        Assert.Same(field, Assert.Single(form.Fields));
        Assert.Equal(2, field.PanelId);
        Assert.Equal(2, form.GetPanelById(3).ParentId);
        Assert.Equal(expected ? parentId : 1, form.GetPanelById(2).ParentId);
        Assert.Equal(4, form.Panels.Count);
        if (expected && beforePanelId.HasValue)
            Assert.True(form.Panels.IndexOf(form.GetPanelById(panelId)) < form.Panels.IndexOf(form.GetPanelById(beforePanelId.Value)));
        repository.Verify(x => x.InsertOrReplaceAsync(form), expected ? Times.Once() : Times.Never());
    }

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
