using JJConsulting.FontAwesome;
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

        Assert.Contains("type=\"text\" class=\"form-control jj-hierarchy-input\" id=\"managerId\" value=\"\"", result.Content);
        Assert.Contains("placeholder=\"(Choose)\"", result.Content);
        Assert.DoesNotContain("input-group-btn", result.Content);
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
        Assert.Contains("type=\"text\" class=\"form-control jj-hierarchy-input\" id=\"managerId\" value=\"Leaf\"", result.Content);
        Assert.Contains("jj-hierarchy-open", result.Content);
        Assert.Contains("btn btn-secondary jj-hierarchy-open", result.Content);
        Assert.Contains("class=\"card mt-2\" id=\"managerId-hierarchy-panel\" hidden", result.Content);
        Assert.Contains("data-id=\"leaf\"", result.Content);
        Assert.Contains("aria-selected=\"true\"", result.Content);
        Assert.Contains("jj-hierarchy-back", result.Content);
        Assert.Contains("jj-hierarchy-clear", result.Content);
        Assert.Contains("<hr", result.Content);
        Assert.Contains("btn btn-link btn-sm jj-hierarchy-close ms-auto", result.Content);
        Assert.Contains(">Close</button>", result.Content);
        Assert.Contains("#cc3344", result.Content);
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
        Assert.DoesNotContain("jj-hierarchy-close", result.Content);
    }

    [Fact]
    public async Task Hierarchy_Modal_Renders_Title_Tree_And_Footer_Actions()
    {
        var hierarchy = CreateHierarchy(PageState.Update);
        hierarchy.ShowAsModal = true;
        hierarchy.ModalTitle = "Manager";

        var result = await hierarchy.GetResultAsync();

        Assert.Contains("data-modal=\"true\"", result.Content);
        Assert.Contains("class=\"modal fade jj-hierarchy-modal\"", result.Content);
        Assert.Contains("class=\"modal-title\"", result.Content);
        Assert.Contains(">Manager</h5>", result.Content);
        Assert.Contains("class=\"modal-body\"", result.Content);
        Assert.Contains("class=\"modal-footer jj-hierarchy-actions\"", result.Content);
        Assert.Contains("jj-hierarchy-back", result.Content);
        Assert.Contains("jj-hierarchy-clear", result.Content);
        Assert.Contains("jj-hierarchy-close ms-auto", result.Content);
        Assert.Contains("btn btn-secondary jj-hierarchy-back", result.Content);
        Assert.Contains("btn btn-secondary jj-hierarchy-clear", result.Content);
        Assert.Contains("btn btn-secondary jj-hierarchy-close", result.Content);
        Assert.DoesNotContain("btn-link jj-hierarchy-back", result.Content);
        Assert.DoesNotContain("<hr", result.Content);
        Assert.DoesNotContain("class=\"card mt-2\"", result.Content);
    }

    [Fact]
    public async Task Hierarchy_Children_Result_Reuses_Node_Html()
    {
        var protection = new DataProtectionService(new EphemeralDataProtectionProvider());
        var route = protection.ProtectObject(new RouteContext(ComponentContext.Hierarchy));
        var context = new DefaultHttpContext();
        context.Request.QueryString = new QueryString($"?fieldName=managerId&parentId=root&routeContext={Uri.EscapeDataString(route)}");
        var hierarchy = CreateHierarchy(PageState.Update, context, protection);

        var result = await hierarchy.GetResultAsync();

        Assert.IsType<ContentComponentResult>(result);
        Assert.Contains("class=\"jj-hierarchy-group\" role=\"group\"", result.Content);
        Assert.Contains("class=\"jj-hierarchy-node\"", result.Content);
        Assert.Contains("data-id=\"child\"", result.Content);
        Assert.Contains("data-parent-id=\"root\"", result.Content);
        Assert.Contains("data-can-expand=\"true\"", result.Content);
        Assert.Contains("jj-hierarchy-toggle", result.Content);
        Assert.Contains("#cc3344", result.Content);
        Assert.DoesNotContain("data-id=\"root\"", result.Content);
    }

    [Fact]
    public async Task Hierarchy_Children_Result_Renders_Empty_Group_For_Leaf()
    {
        var protection = new DataProtectionService(new EphemeralDataProtectionProvider());
        var route = protection.ProtectObject(new RouteContext(ComponentContext.Hierarchy));
        var context = new DefaultHttpContext();
        context.Request.QueryString = new QueryString($"?fieldName=managerId&parentId=leaf&routeContext={Uri.EscapeDataString(route)}");
        var hierarchy = CreateHierarchy(PageState.Update, context, protection);

        var result = await hierarchy.GetResultAsync();

        Assert.IsType<ContentComponentResult>(result);
        Assert.Contains("class=\"jj-hierarchy-group\" role=\"group\"", result.Content);
        Assert.DoesNotContain("jj-hierarchy-node", result.Content);
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
                ShowIcon = true,
                Items =
                [
                    new DataItemValue { Id = "root", Description = "Root" },
                    new DataItemValue { Id = "child", Description = "Child", ParentId = "root", Icon = FontAwesomeIcon.Search, IconColor = "#cc3344" },
                    new DataItemValue { Id = "leaf", Description = "Leaf", ParentId = "child" }
                ]
            }
        };
    }
}
