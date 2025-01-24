using System.Globalization;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Structure;
using JJMasterData.Core.Events.Args;
using JJMasterData.Core.UI.Components;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace JJMasterData.Web.Areas.DataDictionary.Services;

public class LocalizationService(
    IFormElementComponentFactory<JJFormView> formViewFactory,
    LocalizationFormElementFactory localizationFormElementFactory,
    IOptionsSnapshot<RequestLocalizationOptions> requestLocalizationOptions,
    IStringLocalizer<MasterDataResources> stringLocalizer)
{
    public JJFormView GetFormView()
    {
        var supportedCultures = requestLocalizationOptions.Value.SupportedCultures?.ToArray() ?? [];
        var formElement = localizationFormElementFactory.GetFormElement(supportedCultures);

        var formView = formViewFactory.Create(formElement);

        formView.OnAfterInsertAsync += ClearCache;
        formView.OnAfterUpdateAsync += ClearCache;

        return formView;
    }

    private static ValueTask ClearCache(object sender, FormAfterActionEventArgs args)
    {
        MasterDataStringLocalizer.ClearCache();
        return ValueTask.CompletedTask;
    }
    
    
    public async Task<Stream> GetAllStringsStream()
    {
        var localizedStrings = stringLocalizer.GetAllStrings();
        var currentCulture = CultureInfo.CurrentUICulture.Name;
        var memoryStream = new MemoryStream();
        var textWriter = new StreamWriter(memoryStream);
        
        foreach (var localizedString in localizedStrings)
        {
            var line = $"{currentCulture};{localizedString.Name};{localizedString.Value}";
            await textWriter.WriteLineAsync(line);
            await textWriter.FlushAsync();
        }

        memoryStream.Seek(0, SeekOrigin.Begin);
        
        return memoryStream;
    }
}