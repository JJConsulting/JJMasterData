#nullable enable
using System.Threading;
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.DataDictionary.Factories;

public class LocalizationFormEventHandler : FormEventHandlerBase
{
    private IMemoryCache MemoryCache { get; }
    public override string ElementName { get; }
    
    public LocalizationFormEventHandler(IMemoryCache memoryCache,IOptions<JJMasterDataCoreOptions> options)
    {
        MemoryCache = memoryCache;
        ElementName = options.Value.LocalizationTableName;
    }
    public override void OnAfterInsert(object sender, FormAfterActionEventArgs args) => ClearCache();

    public override void OnAfterUpdate(object sender, FormAfterActionEventArgs args) => ClearCache();

    public override void OnBeforeInsert(object sender, FormBeforeActionEventArgs args) =>
        ValidateEspecialChars(args);

    public override void OnBeforeUpdate(object sender, FormBeforeActionEventArgs args) =>
        ValidateEspecialChars(args);
    
    private void ClearCache()
    {
        MemoryCache.Remove($"JJMasterData.Commons.Localization.JJMasterDataResources_localization_strings_{Thread.CurrentThread.CurrentCulture.Name}");
    }

    private static void ValidateEspecialChars(FormBeforeActionEventArgs e)
    {
        if (e.Values == null)
            return;

        if (e.Values.Count == 0)
            return;

        if (e.Values["resourceKey"].ToString()!.Contains("'") ||
            e.Values["resourceKey"].ToString()!.Contains("\"") ||
            e.Values["resourceValue"].ToString()!.Contains("'") ||
            e.Values["resourceValue"].ToString()!.Contains("\""))
        {
            e.Errors.Add("Error", "Character \' not allowed");
        }
    }
}