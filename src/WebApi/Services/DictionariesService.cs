using System.Collections;
using System.Text;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.WebApi.Models;
using Microsoft.Extensions.Localization;

namespace JJMasterData.WebApi.Services;

public class DictionariesService(IDataDictionaryRepository dataDictionaryRepository,
    IEntityRepository entityRepository,
    IStringLocalizer<MasterDataResources> stringLocalizer,
    IHttpContext httpContext,
    ILogger<DictionariesService> logger)
{
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;
    private ILogger<DictionariesService> Logger { get; } = logger;

    /// <summary>
    /// Analisa uma lista de elementos retornando quantos registros precisam ser sincronizados
    /// </summary>
    /// <param name="listSync">Lista de elementos</param>
    /// <param name="showLogInfo">Grava log detalhado de cada operação</param>
    /// <param name="maxRecordsAllowed">
    /// Numero máximo de registros permitidos, 
    /// se ultrapassar esse numero uma exeção será disparada
    /// </param>
    public async Task<DicSyncInfo> GetSyncInfoAsync(DicSyncParam[] listSync, bool showLogInfo, long maxRecordsAllowed = 0)
    {
        var userId = DataHelper.GetCurrentUserId(httpContext, null!);
        if (listSync == null)
            throw new ArgumentNullException(nameof(listSync));

        if (listSync.Length == 0)
            throw new ArgumentException("DicSyncParam invalid");

        var dStart = DateTime.Now;
        var dictionaries = await dataDictionaryRepository.GetFormElementListAsync(true);
        var syncInfo = new DicSyncInfo
        {
            ServerDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
        };
        int totRecords = 0;
        foreach (var os in listSync)
        {
            var dStartObj = DateTime.Now;
            var dictionary = dictionaries.Find(x => x.Name.Equals(os.Name));
            if (dictionary == null)
                throw new JJMasterDataException($"Element {os.Name} not found or not configured for sync");

            var filters = GetSyncInfoFilter(userId, dictionary, os.Filters);
            var info = new DicSyncInfoElement
            {
                Name = os.Name,
                RecordSize = await GetCountAsync(dictionary, filters)
            };
            totRecords += info.RecordSize;

            TimeSpan tsObj = DateTime.Now - dStartObj;
            info.ProcessMilliseconds = tsObj.TotalMilliseconds;
            syncInfo.ListElement.Add(info);

            if (showLogInfo)
            {
                Logger.LogInformation($"- {os.Name}: [{info.RecordSize}] {tsObj.TotalMilliseconds}ms\r\n");
            }

            if (maxRecordsAllowed > 0 && info.RecordSize > maxRecordsAllowed)
            {
                throw new JJMasterDataException($"Number maximum of records exceeded on {os.Name}, contact the administrator.");
            }
        }

        var ts = DateTime.Now - dStart;
        syncInfo.TotalProcessMilliseconds = ts.TotalMilliseconds;

        var message = new StringBuilder();
        message.AppendLine($"UserId: {userId}");
        message.Append(StringLocalizer["Synchronizing"]);
        message.Append(listSync.Length);
        message.Append(StringLocalizer["objects"]);
        message.Append(' ');
        message.AppendLine(" ...");
        message.AppendLine(StringLocalizer["{0} records analyzed in {1}", totRecords, Format.FormatTimeSpan(ts)]);
        Logger.LogInformation(message.ToString());

        if (syncInfo.ListElement.Count == 0)
            throw new KeyNotFoundException(StringLocalizer["No dictionary found"]);

        return syncInfo;
    }

    private async Task<int> GetCountAsync(Element element, Dictionary<string,object?> filters)
    {
        var parameters = new EntityParameters
        {
            Filters = filters,
            RecordsPerPage = 1
        };
        var result = await entityRepository.GetDictionaryListResultAsync(element, parameters);
        return result.TotalOfRecords;
    }
    
    private static Dictionary<string,object?> GetSyncInfoFilter(string? userId, FormElement metadata, Hashtable? metadataFilters)
    {
        var filters = new Dictionary<string,object?>();
        var fields = metadata.Fields;
        if (metadataFilters != null)
        {
            foreach (DictionaryEntry osFilter in metadataFilters)
            {
                if (!fields.Contains(osFilter.Key.ToString()))
                    continue;

                filters.Add(fields[osFilter.Key.ToString()!].Name, osFilter.Value);
            }
        }

        string? fieldApplyUser = metadata.ApiOptions.ApplyUserIdOn;
        
        if (string.IsNullOrEmpty(fieldApplyUser)) 
            return filters;
        if (!filters.TryGetValue(fieldApplyUser, out var filter))
        {
            filters.Add(fieldApplyUser, userId);
        }
        else
        {
            if (!filter!.ToString()!.Equals(userId))
                throw new UnauthorizedAccessException($"Access denied to change user filter on {metadata.Name}");
        }

        return filters;
    }

}
