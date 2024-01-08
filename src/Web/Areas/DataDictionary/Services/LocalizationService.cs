using JJMasterData.Core.DataDictionary.Structure;
using JJMasterData.Core.Events.Args;
using JJMasterData.Core.UI.Components;
using Microsoft.Extensions.Caching.Memory;

namespace JJMasterData.Web.Areas.DataDictionary.Services;

public class LocalizationService(
    IFormElementComponentFactory<JJFormView> formViewFactory,
    LocalizationFormElementFactory localizationFormElementFactory,
    IMemoryCache memoryCache)
{
    private IFormElementComponentFactory<JJFormView> FormViewFactory { get; } = formViewFactory;
    private LocalizationFormElementFactory LocalizationFormElementFactory { get; } = localizationFormElementFactory;
    private IMemoryCache MemoryCache { get; } = memoryCache;

    public JJFormView GetFormView()
    {
        var formElement = LocalizationFormElementFactory.GetFormElement();

        var formView = FormViewFactory.Create(formElement);

        formView.OnAfterInsertAsync += ClearCache;
        formView.OnAfterUpdateAsync += ClearCache;

        return formView;
    }

    private Task ClearCache(object sender, FormAfterActionEventArgs args)
    {
        ClearCache();
        return Task.CompletedTask;
    }

    private void ClearCache()
    {
        MemoryCache.Remove(
            $"JJMasterData.Commons.Localization.MasterDataResources_localization_strings_{Thread.CurrentThread.CurrentCulture.Name}");
    }
}