using JJMasterData.Core.DataDictionary.Structure;
using JJMasterData.Core.Events.Args;
using JJMasterData.Core.UI.Components;
using Microsoft.Extensions.Caching.Memory;

namespace JJMasterData.Web.Areas.DataDictionary.Services;

public class LocalizationService(IFormElementComponentFactory<JJFormView> formViewFactory,LocalizationFormElementFactory localizationFormElementFactory,IMemoryCache memoryCache)
{
    private IFormElementComponentFactory<JJFormView> FormViewFactory { get; } = formViewFactory;
    private LocalizationFormElementFactory LocalizationFormElementFactory { get; } = localizationFormElementFactory;
    private IMemoryCache MemoryCache { get; } = memoryCache;

    public JJFormView GetFormView()
    {
        var formElement = LocalizationFormElementFactory.GetFormElement();

        var formView = FormViewFactory.Create(formElement);

        return formView;
    }
    
    public void OnAfterInsert(object sender, FormAfterActionEventArgs args) => ClearCache();

    public void OnAfterUpdate(object sender, FormAfterActionEventArgs args) => ClearCache();

    public void OnBeforeInsert(object sender, FormBeforeActionEventArgs args) =>
        ValidateEspecialChars(sender, args);

    public void OnBeforeUpdate(object sender, FormBeforeActionEventArgs args) =>
        ValidateEspecialChars(sender, args);
    
    private void ClearCache()
    {
        MemoryCache.Remove($"JJMasterData.Commons.Localization.JJMasterDataResources_localization_strings_{Thread.CurrentThread.CurrentCulture.Name}");
    }

    private static void ValidateEspecialChars(object? sender, FormBeforeActionEventArgs e)
    {
        if (e?.Values == null)
            return;

        if (e.Values.Count == 0)
            return;

        if (e.Values["resourceKey"]!.ToString()!.Contains("'") ||
            e.Values["resourceKey"]!.ToString()!.Contains("\"") ||
            e.Values["resourceValue"]!.ToString()!.Contains("'") ||
            e.Values["resourceValue"]!.ToString()!.Contains("\""))
        {
            e.Errors.Add("Error", "Character \' not allowed");
        }
    }
}