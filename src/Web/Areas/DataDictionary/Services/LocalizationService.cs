using System.Globalization;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Structure;
using JJMasterData.Core.Events.Args;
using JJMasterData.Core.UI.Components;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace JJMasterData.Web.Areas.DataDictionary.Services;

public class LocalizationService(
    IFormElementComponentFactory<JJFormView> formViewFactory,
    LocalizationFormElementFactory localizationFormElementFactory,
    IOptions<RequestLocalizationOptions> requestLocalizationOptions,
    IStringLocalizer<MasterDataResources> stringLocalizer,
    IMemoryCache memoryCache)
{
    private IFormElementComponentFactory<JJFormView> FormViewFactory { get; } = formViewFactory;
    private LocalizationFormElementFactory LocalizationFormElementFactory { get; } = localizationFormElementFactory;
    private IMemoryCache MemoryCache { get; } = memoryCache;

    public JJFormView GetFormView()
    {
        var supportedCultures = requestLocalizationOptions.Value.SupportedCultures?.ToArray() ?? [];
        var formElement = LocalizationFormElementFactory.GetFormElement(supportedCultures);

        var formView = FormViewFactory.Create(formElement);

        formView.OnAfterInsertAsync += ClearCache;
        formView.OnAfterUpdateAsync += ClearCache;

        return formView;
    }

    private ValueTask ClearCache(object sender, FormAfterActionEventArgs args)
    {
        ClearCache();
        return ValueTask.CompletedTask;
    }
    
    public async Task<byte[]> GetAllStringsFile()
    {
        var localizedStrings = stringLocalizer.GetAllStrings();
        var currentCulture = CultureInfo.CurrentUICulture.Name;
        using var memoryStream = new MemoryStream();
        await using TextWriter textWriter = new StreamWriter(memoryStream);
        
        foreach (var localizedString in localizedStrings)
        {
            var line = $"{currentCulture};{localizedString.Name};{localizedString.Value}";
            await textWriter.WriteLineAsync(line);
            await textWriter.FlushAsync();
        }

        return memoryStream.ToArray();
    }

    private void ClearCache()
    {
        MemoryCache.Remove(
            $"JJMasterData.Commons.Localization.MasterDataResources_localization_strings_{Thread.CurrentThread.CurrentCulture.Name}");
    }
}