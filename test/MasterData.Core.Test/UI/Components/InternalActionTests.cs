using System.Reflection;
using System.Text.Json;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Core.UI.Routing;
using JJMasterData.Core.Extensions;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Core.Configuration;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.UI.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace JJMasterData.Core.Test.UI.Components;

public class InternalActionTests
{
    [Theory]
    [InlineData(RelationshipViewType.List, PageState.List)]
    [InlineData(RelationshipViewType.Insert, PageState.Insert)]
    [InlineData(RelationshipViewType.Update, PageState.Update)]
    [InlineData(RelationshipViewType.View, PageState.View)]
    public async Task OpensTargetWithMappedRowAndUserValues(RelationshipViewType viewType, PageState expectedState)
    {
        var parentElement = new FormElement { Name = "requests" };
        var targetElement = new FormElement { Name = "details" };
        var action = new InternalAction { Name = "open-details", ShowTitle = true };
        action.ElementRedirect.ElementNameRedirect = targetElement.Name;
        action.ElementRedirect.ViewType = viewType;
        action.ElementRedirect.RelationFields.Add(new FormActionRelationField
            { InternalField = "customer", RedirectField = "customer_id" });
        action.ElementRedirect.RelationFields.Add(new FormActionRelationField
            { InternalField = "tenant", RedirectField = "tenant_id" });

        var repository = new Mock<IEntityRepository>();
        repository.Setup(r => r.GetFieldsAsync(parentElement, It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(new Dictionary<string, object?> { ["id"] = 42, ["customer"] = "A&B" });
        repository.Setup(r => r.GetFieldsAsync(targetElement, It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(new Dictionary<string, object?> { ["description"] = "Loaded record" });
        using var provider = CreateProvider(targetElement, repository.Object, new DefaultHttpContext());
        var parent = provider.GetRequiredService<IComponentFactory>().FormView.Create(parentElement);
        parent.UserValues["tenant"] = "tenant-1";
        parent.CurrentActionMap = new ActionMap
        {
            ActionName = action.Name, ElementName = parentElement.Name, ActionSource = ActionSource.GridTable,
            PkFieldValues = new Dictionary<string, object> { ["id"] = 42 }
        };

        var child = await CreateChild(parent, action);

        Assert.Equal(targetElement.Name, child.FormElement.Name);
        Assert.Equal(parentElement.Name, child.FormElement.ParentName);
        Assert.Equal(expectedState, child.PageState);
        Assert.Equal("A&B", child.RelationValues["customer_id"]);
        Assert.Equal("tenant-1", child.RelationValues["tenant_id"]);
        Assert.Equal("tenant-1", child.UserValues["tenant"]);
        Assert.True(child.ShowTitle);
        Assert.Null(child.CurrentActionMap);
        Assert.NotEmpty(child.DataPanel.FieldNamePrefix);
        Assert.Equal(viewType is RelationshipViewType.Update or RelationshipViewType.View,
            child.DataPanel.Values.ContainsKey("description"));
    }

    [Fact]
    public async Task SubsequentRequestPreservesTargetStateAndMappedValues()
    {
        var target = new FormElement { Name = "details" };
        var httpContext = new DefaultHttpContext();
        using var provider = CreateProvider(target, Mock.Of<IEntityRepository>(), httpContext);
        var protection = provider.GetRequiredService<JJMasterData.Commons.Security.DataProtectionService>();
        httpContext.Request.ContentType = "application/x-www-form-urlencoded";
        httpContext.Request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            ["form-view-page-state-details"] = ((int)PageState.Update).ToString(),
            ["form-view-relation-values-details"] = protection.ProtectObject(new Dictionary<string, object> { ["customer_id"] = 42 })
        });
        var parent = provider.GetRequiredService<IComponentFactory>().FormView.Create(new FormElement { Name = "requests" });
        var action = new InternalAction { Name = "open-details" };
        action.ElementRedirect.ElementNameRedirect = target.Name;
        action.ElementRedirect.ViewType = RelationshipViewType.Insert;

        var child = await CreateChild(parent, action);

        Assert.Equal(PageState.Update, child.PageState);
        Assert.Equal("42", child.RelationValues["customer_id"].ToString());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RendersTargetStateAndRouteWithinTheExistingFormView(bool showAsModal)
    {
        var target = new FormElement { Name = "details", Title = "Details" };
        var source = new FormElement { Name = "requests" };
        var action = new InternalAction { Name = "open-details" };
        action.ElementRedirect.ElementNameRedirect = target.Name;
        action.ElementRedirect.ViewType = RelationshipViewType.Insert;
        action.ElementRedirect.ShowAsModal = showAsModal;
        source.Options.GridToolbarActions.Add(action);
        var context = new DefaultHttpContext();
        using var provider = CreateProvider(target, Mock.Of<IEntityRepository>(), context);
        var protection = provider.GetRequiredService<JJMasterData.Commons.Security.DataProtectionService>();
        context.Request.ContentType = "application/x-www-form-urlencoded";
        context.Request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            ["current-action-map-requests"] = protection.ProtectObject(new ActionMap
                { ActionName = action.Name, ElementName = source.Name, ActionSource = ActionSource.GridToolbar })
        });
        var parent = provider.GetRequiredService<IComponentFactory>().FormView.Create(source);

        var result = Assert.IsAssignableFrom<HtmlComponentResult>(await parent.GetResultAsync());
        var html = result.HtmlBuilder.ToString();

        Assert.Contains("form-view-page-state-details", html);
        Assert.Contains("current-action-map-details", html);
        Assert.Contains("form-view-route-context-details", html);
        Assert.DoesNotContain("iframe", html, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(!showAsModal, html.Contains("current-action-map-requests"));
    }

    [Theory]
    [InlineData("save", true, true, RelationshipViewType.Insert, PageState.Insert)]
    [InlineData("cancel", true, true, RelationshipViewType.Insert, PageState.Insert)]
    [InlineData("back", true, true, RelationshipViewType.Insert, PageState.Insert)]
    [InlineData("save", false, true, RelationshipViewType.Insert, PageState.Insert)]
    [InlineData("cancel", false, true, RelationshipViewType.Insert, PageState.Insert)]
    [InlineData("back", false, true, RelationshipViewType.Insert, PageState.Insert)]
    [InlineData("save", true, false, RelationshipViewType.Insert, PageState.Insert)]
    [InlineData("save", false, false, RelationshipViewType.Insert, PageState.Insert)]
    [InlineData("save", true, true, RelationshipViewType.List, PageState.Insert)]
    [InlineData("save", false, true, RelationshipViewType.List, PageState.Insert)]
    [InlineData("save", true, true, RelationshipViewType.List, PageState.Update)]
    [InlineData("save", false, true, RelationshipViewType.List, PageState.Update)]
    [InlineData("cancel", true, true, RelationshipViewType.List, PageState.Update)]
    [InlineData("cancel", false, true, RelationshipViewType.List, PageState.Update)]
    [InlineData("back", true, true, RelationshipViewType.List, PageState.View)]
    [InlineData("back", false, true, RelationshipViewType.List, PageState.View)]
    [InlineData("back", true, true, RelationshipViewType.List, PageState.List)]
    [InlineData("back", false, true, RelationshipViewType.List, PageState.List)]
    public async Task NonModalCompletionReturnsToExpectedDictionary(string actionName, bool ajax, bool valid,
        RelationshipViewType entryType, PageState recordState)
    {
        var target = new FormElement { Name = "details", Title = "Details" };
        if (!valid)
            target.Fields.Add(new FormElementField { Name = "Description", Label = "Description", IsRequired = true });
        var source = new FormElement { Name = "requests", Title = "Requests" };
        var action = new InternalAction { Name = "open-details" };
        action.ElementRedirect.ElementNameRedirect = target.Name;
        action.ElementRedirect.ViewType = entryType;
        action.ElementRedirect.ShowAsModal = false;
        source.Options.GridToolbarActions.Add(action);
        var context = new DefaultHttpContext();
        var repository = new Mock<IEntityRepository>();
        repository.Setup(r => r.GetDictionaryListResultAsync(It.IsAny<Element>(), It.IsAny<EntityParameters>(), true))
            .ReturnsAsync(new DictionaryListResult(new List<Dictionary<string, object?>>(), 0));
        repository.Setup(r => r.UpdateAsync(target, It.IsAny<Dictionary<string, object?>>())).ReturnsAsync(1);
        using var provider = CreateProvider(target, repository.Object, context);
        var protection = provider.GetRequiredService<JJMasterData.Commons.Security.DataProtectionService>();
        context.Request.ContentType = "application/x-www-form-urlencoded";
        context.Request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            ["current-action-map-requests"] = protection.ProtectObject(new ActionMap
                { ActionName = action.Name, ElementName = source.Name, ActionSource = ActionSource.GridToolbar }),
            ["current-action-map-details"] = protection.ProtectObject(new ActionMap
                { ActionName = actionName, ElementName = target.Name, ActionSource = ActionSource.FormToolbar }),
            ["form-view-page-state-details"] = ((int)recordState).ToString()
        });
        if (ajax)
        {
            target.ParentName = source.Name;
            context.Request.QueryString = new QueryString("?routeContext=" +
                Uri.EscapeDataString(protection.ProtectObject(RouteContext.FromFormElement(target, ComponentContext.FormViewReload))));
        }
        var parent = provider.GetRequiredService<IComponentFactory>().FormView.Create(source);

        var result = await parent.GetResultAsync();

        if (!valid)
        {
            var html = Assert.IsAssignableFrom<HtmlComponentResult>(result).HtmlBuilder.ToString();
            Assert.Contains("form-view-page-state-details", html);
            Assert.Contains("Description", html);
        }
        else if (entryType is RelationshipViewType.List && recordState is not PageState.List)
        {
            var html = Assert.IsAssignableFrom<HtmlComponentResult>(result).HtmlBuilder.ToString();
            Assert.Contains("form-view-page-state-details", html);
            Assert.Contains("details-back", html);
            Assert.True(html.IndexOf("grid-view-table-details", StringComparison.Ordinal) <
                        html.IndexOf("details-back", StringComparison.Ordinal));
            repository.Verify(r => r.GetDictionaryListResultAsync(target, It.IsAny<EntityParameters>(), true), Times.Once());
            repository.Verify(r => r.GetDictionaryListResultAsync(source, It.IsAny<EntityParameters>(), true), Times.Never());
        }
        else if (ajax)
        {
            var json = Assert.IsType<JsonComponentResult>(result);
            using var document = JsonDocument.Parse(json.Content);
            var callback = document.RootElement.GetProperty("jsCallback").GetString()!;
            Assert.StartsWith($"FormViewHelper.setPageState('requests','{(int)PageState.List}',", callback);
            Assert.DoesNotContain("details", callback);
        }
        else
        {
            var html = Assert.IsAssignableFrom<HtmlComponentResult>(result).HtmlBuilder.ToString();
            Assert.Contains("form-view-page-state-requests", html);
            Assert.DoesNotContain("form-view-page-state-details", html);
            Assert.Equal(PageState.List, parent.PageState);
            Assert.Null(parent.CurrentActionMap);
        }
        repository.Verify(r => r.InsertAsync(target, It.IsAny<Dictionary<string, object?>>()),
            actionName == "save" && valid && recordState is PageState.Insert ? Times.Once() : Times.Never());
        repository.Verify(r => r.UpdateAsync(target, It.IsAny<Dictionary<string, object?>>()),
            actionName == "save" && valid && recordState is PageState.Update ? Times.Once() : Times.Never());
    }

    [Fact]
    public async Task InternalBackAndCancelUseServerActionsWhenTargetDefaultsToModal()
    {
        var target = new FormElement { Name = "details" };
        target.Options.GridTableActions.EditAction.ShowAsModal = true;
        target.Options.GridTableActions.ViewAction.ShowAsModal = true;
        using var provider = CreateProvider(target, Mock.Of<IEntityRepository>(), new DefaultHttpContext());
        var parent = provider.GetRequiredService<IComponentFactory>().FormView.Create(new FormElement { Name = "requests" });
        var action = new InternalAction { Name = "open-details" };
        action.ElementRedirect.ElementNameRedirect = target.Name;
        action.ElementRedirect.ViewType = RelationshipViewType.Insert;
        action.ElementRedirect.ShowAsModal = false;
        var child = await CreateChild(parent, action);
        var buttons = provider.GetRequiredService<ActionButtonFactory>();
        var state = new FormStateData(new Dictionary<string, object?>(), PageState.View);

        foreach (var completionAction in new BasicAction[]
                 { target.Options.FormToolbarActions.CancelAction, target.Options.FormToolbarActions.BackAction })
        {
            var button = buttons.CreateFormToolbarButton(completionAction, state, child);
            Assert.StartsWith("ActionHelper.executeAction(", button.OnClientClick);
            Assert.Equal("false", button.Attributes["data-is-modal"]);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task InternalListHasBackButtonBelowGridOnlyOutsideModal(bool modal)
    {
        var target = new FormElement { Name = "details" };
        var source = new FormElement { Name = "requests" };
        var action = new InternalAction { Name = "open-list" };
        action.ElementRedirect.ElementNameRedirect = target.Name;
        action.ElementRedirect.ViewType = RelationshipViewType.List;
        action.ElementRedirect.ShowAsModal = modal;
        source.Options.GridToolbarActions.Add(action);
        var repository = new Mock<IEntityRepository>();
        repository.Setup(r => r.GetDictionaryListResultAsync(target, It.IsAny<EntityParameters>(), true))
            .ReturnsAsync(new DictionaryListResult(new List<Dictionary<string, object?>>(), 0));
        var context = new DefaultHttpContext();
        using var provider = CreateProvider(target, repository.Object, context);
        var protection = provider.GetRequiredService<JJMasterData.Commons.Security.DataProtectionService>();
        context.Request.ContentType = "application/x-www-form-urlencoded";
        context.Request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            ["current-action-map-requests"] = protection.ProtectObject(new ActionMap
                { ActionName = action.Name, ElementName = source.Name, ActionSource = ActionSource.GridToolbar })
        });
        var parent = provider.GetRequiredService<IComponentFactory>().FormView.Create(source);

        var html = Assert.IsAssignableFrom<HtmlComponentResult>(await parent.GetResultAsync()).HtmlBuilder.ToString();

        Assert.Equal(!modal, html.Contains("details-back"));
        if (!modal)
            Assert.True(html.IndexOf("grid-view-table-details", StringComparison.Ordinal) <
                        html.IndexOf("details-back", StringComparison.Ordinal));
    }

    private static Task<JJFormView> CreateChild(JJFormView parent, InternalAction action) =>
        (Task<JJFormView>)typeof(JJFormView)
            .GetMethod("CreateInternalActionFormView", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(parent, [action])!;

    private static ServiceProvider CreateProvider(FormElement target, IEntityRepository repository, HttpContext context)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMemoryCache();
        services.AddLocalization();
        services.AddRouting();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["JJMasterData:ConnectionString"] = "Server=(local);Database=master;Integrated Security=true" }).Build());
        services.AddJJMasterDataCore();
        services.AddSingleton<IHttpContextAccessor>(new HttpContextAccessor { HttpContext = context });
        services.AddSingleton(repository);
        services.AddSingleton(Mock.Of<Microsoft.AspNetCore.Mvc.IUrlHelper>());
        var dictionary = new Mock<IDataDictionaryRepository>();
        dictionary.Setup(r => r.GetFormElementAsync(target.Name)).ReturnsAsync(target);
        services.AddSingleton(dictionary.Object);
        return services.BuildServiceProvider();
    }
}
