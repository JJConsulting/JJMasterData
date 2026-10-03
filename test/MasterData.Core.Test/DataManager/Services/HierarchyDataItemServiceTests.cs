using System.Text.Json;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Resources;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;

namespace JJMasterData.Core.Test.DataManager.Services;

public class HierarchyDataItemServiceTests
{
    [Fact]
    public void HierarchyProperties_Serialize_And_DeepCopy()
    {
        var dataItem = CreateDataItem();
        dataItem.ElementMap = new DataElementMap
        {
            ElementName = "Employees",
            IdFieldName = "Id",
            ParentIdFieldName = "ManagerId"
        };
        dataItem.ShowAsModal = true;

        var json = JsonSerializer.Serialize(dataItem);
        var deserialized = JsonSerializer.Deserialize<FormElementDataItem>(json)!;
        var copy = dataItem.DeepCopy();

        Assert.Equal("root", deserialized.Items![0].Id);
        Assert.Equal("root", deserialized.Items[1].ParentId);
        Assert.Equal("ManagerId", deserialized.ElementMap!.ParentIdFieldName);
        Assert.Contains("\"showAsModal\":true", json);
        Assert.True(deserialized.ShowAsModal);
        Assert.True(copy.ShowAsModal);
        Assert.NotSame(dataItem.Items, copy.Items);
        Assert.NotSame(dataItem.ElementMap, copy.ElementMap);
    }

    [Fact]
    public async Task GetHierarchyValues_Filters_Roots_And_Children()
    {
        var service = CreateService();
        var dataItem = CreateDataItem();
        var state = new FormStateData([], PageState.Update);

        var roots = await service.GetHierarchyValuesAsync(dataItem, new DataQuery(state, null));
        var children = await service.GetHierarchyValuesAsync(dataItem,
            new DataQuery(state, null) { ParentId = "root" });

        Assert.Collection(roots, root => Assert.Equal("root", root.Id));
        Assert.Collection(children, child => Assert.Equal("child", child.Id));
    }

    [Fact]
    public async Task GetHierarchyPath_Returns_Ancestors_From_Root_To_Selected_Item()
    {
        var service = CreateService();
        var state = new FormStateData([], PageState.Update);

        var path = await service.GetHierarchyPathAsync(CreateDataItem(),
            new DataQuery(state, null) { SearchId = "leaf" });

        Assert.Equal(["root", "child", "leaf"], path.Select(item => item.Id));
    }

    [Fact]
    public async Task GetHierarchyPath_Rejects_Cycles()
    {
        var service = CreateService();
        var state = new FormStateData([], PageState.Update);
        var dataItem = new FormElementDataItem
        {
            Items =
            [
                new DataItemValue { Id = "a", Description = "A", ParentId = "b" },
                new DataItemValue { Id = "b", Description = "B", ParentId = "a" }
            ]
        };

        await Assert.ThrowsAsync<JJMasterDataException>(() => service.GetHierarchyPathAsync(dataItem,
            new DataQuery(state, null) { SearchId = "a" }));
    }

    private static FormElementDataItem CreateDataItem() => new()
    {
        Items =
        [
            new DataItemValue { Id = "root", Description = "Root" },
            new DataItemValue { Id = "child", Description = "Child", ParentId = "root" },
            new DataItemValue { Id = "leaf", Description = "Leaf", ParentId = "child" }
        ]
    };

    private static DataItemService CreateService()
    {
        var entityRepository = Mock.Of<IEntityRepository>();
        var localizer = CreateLocalizer();
        var hierarchyService = new HierarchyService(
            entityRepository,
            null!,
            null!,
            localizer,
            Mock.Of<ILogger<HierarchyService>>());

        return new DataItemService(
            entityRepository,
            null!,
            null!,
            localizer,
            Mock.Of<ILogger<DataItemService>>(),
            hierarchyService);
    }

    internal static IStringLocalizer<MasterDataResources> CreateLocalizer()
    {
        var localizer = new Mock<IStringLocalizer<MasterDataResources>>();
        localizer.Setup(value => value[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));
        return localizer.Object;
    }
}
