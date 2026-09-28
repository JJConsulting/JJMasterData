using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Security;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Routing;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace JJMasterData.Core.Test.UI.Components;

public class JJHierarchyTests
{
    [Fact]
    public async Task Hierarchy_Empty_Renders_Compact_Picker()
    {
        var hierarchy = CreateHierarchy(PageState.Insert);
        hierarchy.SelectedValue = null;

        var result = await hierarchy.GetResultAsync();

        Assert.Contains("type=\"text\" class=\"form-control\" id=\"managerId\" value=\"\"", result.Content);
        Assert.Contains("placeholder=\"(Choose)\"", result.Content);
        Assert.Contains("aria-expanded=\"false\"", result.Content);
        Assert.Contains("class=\"card mt-2\"", result.Content);
        Assert.Contains("class=\"card-body\"", result.Content);
    }

    [Fact]
    public async Task Hierarchy_Renders_Selected_Path_And_Actions()
    {
        var hierarchy = CreateHierarchy(PageState.Update);

        var result = await hierarchy.GetResultAsync();

        Assert.Contains("role=\"tree\"", result.Content);
        Assert.Contains("type=\"hidden\" name=\"managerId\" id=\"managerId-value\" value=\"leaf\"", result.Content);
        Assert.Contains("type=\"text\" class=\"form-control\" id=\"managerId\" value=\"Leaf\"", result.Content);
        Assert.Contains("jj-hierarchy-open", result.Content);
        Assert.Contains("btn btn-secondary jj-hierarchy-open", result.Content);
        Assert.Contains("class=\"card mt-2\" id=\"managerId-hierarchy-panel\" hidden", result.Content);
        Assert.Contains("data-id=\"leaf\"", result.Content);
        Assert.Contains("aria-selected=\"true\"", result.Content);
        Assert.Contains("jj-hierarchy-back", result.Content);
        Assert.Contains("jj-hierarchy-clear", result.Content);
    }

    [Fact]
    public async Task Hierarchy_View_Renders_Path_Without_Actions()
    {
        var hierarchy = CreateHierarchy(PageState.View);
        hierarchy.ShowSelectedPathOnly = true;

        var result = await hierarchy.GetResultAsync();

        Assert.Contains("data-id=\"root\"", result.Content);
        Assert.Contains("data-id=\"leaf\"", result.Content);
        Assert.DoesNotContain("jj-hierarchy-actions", result.Content);
        Assert.DoesNotContain("jj-hierarchy-open", result.Content);
    }

    [Fact]
    public async Task Hierarchy_Children_Result_Uses_Client_Json_Contract()
    {
        var protection = new DataProtectionService(new EphemeralDataProtectionProvider());
        var route = protection.ProtectObject(new RouteContext(ComponentContext.Hierarchy));
        var context = new DefaultHttpContext();
        context.Request.QueryString = new QueryString($"?fieldName=managerId&parentId=root&routeContext={Uri.EscapeDataString(route)}");
        var hierarchy = CreateHierarchy(PageState.Update, context, protection);

        var result = await hierarchy.GetResultAsync();

        Assert.Contains("\"id\":\"child\"", result.Content);
        Assert.Contains("\"parentId\":\"root\"", result.Content);
        Assert.Contains("\"canExpand\":true", result.Content);
    }

    private static JJHierarchy CreateHierarchy(
        PageState pageState,
        DefaultHttpContext? httpContext = null,
        DataProtectionService? protection = null)
    {
        var httpContextAccessor = new HttpContextAccessor { HttpContext = httpContext ?? new DefaultHttpContext() };
        var localizer = JJMasterData.Core.Test.DataManager.Services.HierarchyDataItemServiceTests.CreateLocalizer();
        var entityRepository = Mock.Of<IEntityRepository>();
        var hierarchyService = new HierarchyService(
            entityRepository,
            null!,
            null!,
            localizer,
            Mock.Of<ILogger<HierarchyService>>());
        var service = new DataItemService(
            entityRepository,
            null!,
            null!,
            localizer,
            Mock.Of<ILogger<DataItemService>>(),
            hierarchyService);

        return new JJHierarchy(
            httpContextAccessor,
            protection ?? new DataProtectionService(new EphemeralDataProtectionProvider()),
            service,
            localizer)
        {
            Name = "managerId",
            FieldName = "managerId",
            ElementName = "Employees",
            Visible = true,
            SelectedValue = "leaf",
            FormStateData = new FormStateData([], pageState),
            DataItem = new FormElementDataItem
            {
                Items =
                [
                    new DataItemValue { Id = "root", Description = "Root" },
                    new DataItemValue { Id = "child", Description = "Child", ParentId = "root" },
                    new DataItemValue { Id = "leaf", Description = "Leaf", ParentId = "child" }
                ]
            }
        };
    }
}
