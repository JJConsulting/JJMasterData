using JJMasterData.Commons.Security;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.UI.Components;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;

namespace JJMasterData.Core.Test.UI.Components;

public class GridPreferencesTests
{
    [Fact]
    public void CustomGridName_UsesOneDocumentAndPreservesOtherPreferences()
    {
        var saved = new GridPreferences
        {
            Settings = new GridSettings { RecordsPerPage = 25 },
            Filter = new() { ["Name"] = "Alice" },
            Order = "Name asc"
        };
        var store = new Mock<IGridPreferenceStore>(MockBehavior.Strict);
        store.Setup(s => s.Get("secondary")).Returns(saved);
        store.Setup(s => s.Set("secondary", It.IsAny<GridPreferences>()));
        var (grid, factory, _) = CreateGrid(store.Object);
        grid.Name = "secondary";

        factory.SetGridUiOptions(grid, new GridUI { MaintainValuesOnLoad = true });
        grid.CurrentPage = 4;

        Assert.Same(saved.Settings, grid.CurrentSettings);
        store.Verify(s => s.Get("secondary"), Times.Exactly(2));
        store.Verify(s => s.Set("secondary", It.Is<GridPreferences>(p =>
            p.CurrentPage == 4 && p.Settings == saved.Settings && p.Filter == saved.Filter && p.Order == "Name asc")), Times.Once);
        store.VerifyNoOtherCalls();
    }

    [Fact]
    public void Factory_RestoresSavedSettingsWithoutWritingDefaults()
    {
        var store = new Mock<IGridPreferenceStore>(MockBehavior.Strict);
        var saved = new GridSettings { RecordsPerPage = 25, IsCompact = false };
        store.Setup(s => s.Get("customers")).Returns(new GridPreferences { Settings = saved });
        var (grid, factory, _) = CreateGrid(store.Object);

        factory.SetGridUiOptions(grid, new GridUI { MaintainValuesOnLoad = true, RecordsPerPage = 10 });

        Assert.Same(saved, grid.CurrentSettings);
        store.Verify(s => s.Get("customers"), Times.Once);
        store.VerifyNoOtherCalls();
    }

    [Fact]
    public void Factory_UsesDictionaryDefaultsWithoutPersistingThem()
    {
        var store = new Mock<IGridPreferenceStore>(MockBehavior.Strict);
        store.Setup(s => s.Get("customers")).Returns((GridPreferences?)null);
        var (grid, factory, _) = CreateGrid(store.Object);

        factory.SetGridUiOptions(grid, new GridUI { MaintainValuesOnLoad = true, RecordsPerPage = 10, ShowBorder = true });

        Assert.Equal(10, grid.CurrentSettings.RecordsPerPage);
        Assert.True(grid.CurrentSettings.ShowBorder);
        store.Verify(s => s.Get("customers"), Times.Once);
        store.VerifyNoOtherCalls();
    }

    [Fact]
    public void DisabledPreferences_DoNotReadOrWriteStore()
    {
        var store = new Mock<IGridPreferenceStore>(MockBehavior.Strict);
        var (grid, factory, _) = CreateGrid(store.Object);
        factory.SetGridUiOptions(grid, new GridUI { MaintainValuesOnLoad = false, RecordsPerPage = 10 });
        Assert.Equal(10, grid.CurrentSettings.RecordsPerPage);
        grid.CurrentSettings = new GridSettings { RecordsPerPage = 20 };
        Assert.Equal(20, grid.CurrentSettings.RecordsPerPage);
        grid.CurrentPage = 3;
        Assert.Equal(3, grid.CurrentPage);
        store.VerifyNoOtherCalls();
    }

    [Fact]
    public void ConfigAction_PersistsSubmittedDisplaySettings()
    {
        var store = new Mock<IGridPreferenceStore>();
        var (grid, factory, context) = CreateGrid(store.Object);
        var protection = grid.DataProtectionService;
        var actionMap = protection.ProtectObject(new ActionMap
        {
            ElementName = "Customers", ActionName = ConfigAction.ActionName, ActionSource = ActionSource.GridToolbar
        });
        context.Request.ContentType = "application/x-www-form-urlencoded";
        context.Request.Form = new FormCollection(new Dictionary<string, StringValues>
        {
            ["grid-view-action-map-customers"] = actionMap,
            ["customers-table-regperpage"] = "25",
            ["customers-table-totalpagebuttons"] = "5",
            ["customers-table-rowstriped"] = "1",
            ["customers-table-is-compact"] = "1"
        });

        factory.SetGridUiOptions(grid, new GridUI { MaintainValuesOnLoad = true });
        Assert.Equal(25, grid.CurrentSettings.RecordsPerPage);
        store.Verify(s => s.Set("customers", It.Is<GridPreferences>(preferences =>
            preferences.Settings != null && preferences.Settings.RecordsPerPage == 25 &&
            preferences.Settings.ShowRowStriped && preferences.Settings.IsCompact)), Times.Once);
    }

    [Fact]
    public void PostedPage_TakesPrecedenceOverStoredPage()
    {
        var store = new Mock<IGridPreferenceStore>(MockBehavior.Strict);
        store.Setup(s => s.Get("customers")).Returns((GridPreferences?)null);
        store.Setup(s => s.Set("customers", It.Is<GridPreferences>(p => p.CurrentPage == 4)));
        var (grid, _, context) = CreateGrid(store.Object);
        grid.MaintainValuesOnLoad = true;
        context.Request.ContentType = "application/x-www-form-urlencoded";
        context.Request.Form = new FormCollection(new Dictionary<string, StringValues> { ["grid-view-page-customers"] = "4" });
        Assert.Equal(4, grid.CurrentPage);
        store.Verify(s => s.Get("customers"), Times.Once);
        store.Verify(s => s.Set("customers", It.Is<GridPreferences>(p => p.CurrentPage == 4)), Times.Once);
        store.VerifyNoOtherCalls();
    }

    [Fact]
    public void FirstLoad_RestoresPageFromCustomStore()
    {
        var store = new Mock<IGridPreferenceStore>();
        store.Setup(s => s.Get("customers")).Returns(new GridPreferences { CurrentPage = 3 });
        var (grid, _, _) = CreateGrid(store.Object);
        grid.MaintainValuesOnLoad = true;
        Assert.Equal(3, grid.CurrentPage);
        store.Verify(s => s.Get("customers"), Times.Once);
    }

    private static (JJGridView Grid, GridViewFactory Factory, DefaultHttpContext Context) CreateGrid(IGridPreferenceStore store)
    {
        var context = new DefaultHttpContext();
        var accessor = new HttpContextAccessor { HttpContext = context };
        var element = new FormElement { Name = "Customers" };
        var grid = new JJGridView(element, accessor, null!, new DataProtectionService(new EphemeralDataProtectionProvider()), null!, null!, null!, null!, null!, null!,
            null!, null!, null!, null!, null!, store);
        var factory = new GridViewFactory(accessor, null!, null!, null!, null!, null!, null!, null!, null!,
            null!, null!, null!, null!, null!, null!, null!, store);
        return (grid, factory, context);
    }
}
