#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.DataDictionary.Services;

public class DataDictionaryLocalizationService(
    IDataDictionaryRepository dataDictionaryRepository,
    IStringLocalizer<MasterDataResources> stringLocalizer,
    ILogger<DataDictionaryLocalizationService>? logger = null,
    DataItemService? dataItemService = null)
{
    private static readonly ResourceManager ResourceManager = new(typeof(MasterDataResources));
    private static readonly string InvariantResourceName = $"{typeof(MasterDataResources).FullName}.resources";

    public async Task<IReadOnlyCollection<string>> GetFormElementLocalizationKeysAsync()
    {
        var keys = new HashSet<string>(StringComparer.Ordinal);
        var formElementList = await dataDictionaryRepository.GetFormElementListAsync();

        foreach (var formElement in formElementList)
        {
            AddKey(keys, formElement.Title);
            AddKey(keys, formElement.SubTitle);

            foreach (var panel in formElement.Panels)
            {
                AddKey(keys, panel.Title);
                AddKey(keys, panel.SubTitle);
            }

            foreach (var relationship in formElement.Relationships)
            {
                AddKey(keys, relationship.Panel?.Title);
                AddKey(keys, relationship.Panel?.SubTitle);
            }

            foreach (var field in formElement.Fields)
            {
                AddKey(keys, field.Name);
                AddKey(keys, field.Label);
                AddKey(keys, field.HelpDescription);

                if (field.Component is FormComponent.ComboBox && field.DataItem?.EnableLocalization == true)
                {
                    try
                    {
                        await AddDataItemAsync(keys, field.DataItem, formElement.ConnectionId);
                    }
                    catch (Exception e)
                    {
                        logger?.LogError(e, "Error adding data item localization keys");
                    }
                }
                
                AddActionKeys(keys, field.Actions);
            }

            AddActionKeys(keys, formElement.Options.GridToolbarActions);
            AddActionKeys(keys, formElement.Options.FormToolbarActions);
            AddActionKeys(keys, formElement.Options.GridTableActions);
        }

        return keys.OrderBy(static x => x, StringComparer.Ordinal).ToArray();
    }

    private async Task AddDataItemAsync(HashSet<string> keys, FormElementDataItem dataItem, Guid? connectionId)
    {
        if (dataItem.HasItems())
        {
            foreach (var item in dataItem.Items!)
            {
                AddKey(keys, item.Description);
            }

            return;
        }

        if (!dataItem.HasSqlCommand() || dataItemService is null)
            return;

        var values = await dataItemService.GetValuesAsync(
            dataItem,
            new DataQuery(new FormStateData(PageState.List), connectionId));

        foreach (var item in values)
        {
            AddKey(keys, item.Description);
        }
    }

    public static IReadOnlyCollection<string> GetCommonsResourceKeys()
    {
        var keys = new HashSet<string>(StringComparer.Ordinal);

        foreach (var culture in GetResourceCultures())
        {
            ResourceSet? resourceSet;
            try
            {
                resourceSet = ResourceManager.GetResourceSet(culture, true, false);
            }
            catch (MissingManifestResourceException)
            {
                continue;
            }

            if (resourceSet == null)
                continue;

            foreach (DictionaryEntry entry in resourceSet)
            {
                if (entry.Key is string key)
                    keys.Add(key);
            }
        }

        return keys.OrderBy(static x => x, StringComparer.Ordinal).ToArray();
    }

    public async Task<List<string>> GetAllLocalizationKeysAsync()
    {
        var keys = new HashSet<string>(StringComparer.Ordinal);

        foreach (var key in await GetFormElementLocalizationKeysAsync())
            keys.Add(key);

        foreach (var key in GetCommonsResourceKeys())
            keys.Add(key);

        return keys.OrderBy(static x => x, StringComparer.Ordinal).ToList();
    }

    public async Task<Dictionary<string, string>> GetLocalizationDictionaryAsync(CultureInfo culture)
    {
        var dictionary = new Dictionary<string, string>(StringComparer.Ordinal);
        var keys = await GetAllLocalizationKeysAsync();

        foreach (var key in keys)
        {
            dictionary[key] = TranslateKey(key, culture);
        }

        return dictionary;
    }

    private static List<CultureInfo> GetResourceCultures()
    {
        var assembly = typeof(MasterDataResources).Assembly;
        var resourceNames = assembly.GetManifestResourceNames().ToHashSet(StringComparer.Ordinal);
        var cultures = new HashSet<CultureInfo>();

        if (resourceNames.Contains(InvariantResourceName))
            cultures.Add(CultureInfo.InvariantCulture);

        var basePath = Path.GetDirectoryName(assembly.Location);
        if (!string.IsNullOrWhiteSpace(basePath) && Directory.Exists(basePath))
        {
            var satelliteAssemblyName = $"{assembly.GetName().Name}.resources.dll";
            foreach (var cultureDirectory in Directory.EnumerateDirectories(basePath))
            {
                var cultureName = Path.GetFileName(cultureDirectory);
                var satelliteAssemblyPath = Path.Combine(cultureDirectory, satelliteAssemblyName);
                if (!File.Exists(satelliteAssemblyPath))
                    continue;

                try
                {
                    cultures.Add(CultureInfo.GetCultureInfo(cultureName));
                }
                catch (CultureNotFoundException)
                {
                }
            }
        }

        return cultures.ToList();
    }

    private static void AddActionKeys(HashSet<string> keys, IEnumerable<BasicAction> actions)
    {
        foreach (var action in actions)
        {
            AddKey(keys, action.Name);
            AddKey(keys, action.Text);
            AddKey(keys, action.Tooltip);

            if (action is IModalAction modalAction)
                AddKey(keys, modalAction.ModalTitle);

            if (action is UrlRedirectAction urlRedirectAction)
                AddKey(keys, urlRedirectAction.ModalTitle);
        }
    }

    private static void AddKey(HashSet<string> keys, string? key)
    {
        if (!string.IsNullOrWhiteSpace(key))
            keys.Add(key!);
    }

    private string TranslateKey(string key, CultureInfo culture)
    {
        var originalUiCulture = CultureInfo.CurrentUICulture;
        var originalCulture = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.CurrentCulture = culture;

            var localized = stringLocalizer[key];
            if (localized.ResourceNotFound || string.IsNullOrWhiteSpace(localized.Value))
                return key;

            return localized.Value;
        }
        finally
        {
            CultureInfo.CurrentUICulture = originalUiCulture;
            CultureInfo.CurrentCulture = originalCulture;
        }
    }
}
