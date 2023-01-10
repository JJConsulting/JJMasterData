using JJMasterData.Commons.Language;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using System.Collections;
using System.Collections.Immutable;
using System.Text;
using JJMasterData.Commons.Dao.Entity.Abstractions;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.WebApi.Models;

namespace JJMasterData.WebApi.Services;

public class DictionariesService
{
    public ILogger<DictionariesService> Logger { get; }
    private readonly IEntityRepository _entityRepository;
    private readonly IDataDictionaryRepository _dataDictionaryRepository;

    public DictionariesService(
        IDataDictionaryRepository dataDictionaryRepository,
        IEntityRepository entityRepository,
        ILogger<DictionariesService> logger
    )
    {
        Logger = logger;
        _dataDictionaryRepository = dataDictionaryRepository;
        _entityRepository = entityRepository;
    }

    /// <summary>
    /// Analisa uma lista de elementos retornando quantos registros precisam ser sincronizados
    /// </summary>
    /// <param name="userId">Id do Usuários</param>
    /// <param name="listSync">Lista de elementos</param>
    /// <param name="showLogInfo">Grava log detalhado de cada operação</param>
    /// <param name="maxRecordsAllowed">
    /// Numero máximo de registros permitidos, 
    /// se ultrapassar esse numero uma exeção será disparada
    /// </param>
    public DicSyncInfo GetSyncInfo(string userId, DicSyncParam[] listSync, bool showLogInfo, long maxRecordsAllowed = 0)
    {
        if (listSync == null)
            throw new ArgumentNullException(nameof(DicSyncParam));

        if (listSync.Length == 0)
            throw new ArgumentException(Translate.Key("DicSyncParam invalid"));

        var dStart = DateTime.Now;
        var dictionaries = _dataDictionaryRepository.GetMetadataList(true).ToImmutableList();
        var syncInfo = new DicSyncInfo
        {
            ServerDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
        };
        int totRecords = 0;
        foreach (var dicSync in listSync)
        {
            var dStartObj = DateTime.Now;
            var dictionary = dictionaries?.First(x => x.Table.Name.Equals(dicSync.Name));
            if (dictionary == null)
                throw new JJMasterDataException(Translate.Key("Dictionary {0} not found or not configured for REST API.",
                    dicSync.Name));

            var filters = GetSyncInfoFilter(userId, dictionary, dicSync.Filters);
            var info = new DicSyncInfoElement
            {
                Name = dicSync.Name,
                RecordSize = _entityRepository.GetCount(dictionary.Table, filters)
            };
            totRecords += info.RecordSize;

            TimeSpan tsObj = DateTime.Now - dStartObj;
            info.ProcessMilliseconds = tsObj.TotalMilliseconds;
            syncInfo.ListElement.Add(info);

            if (showLogInfo)
            {
                Logger.LogInformation("{dictionaryName}: [{recordSize}] {totalMilliseconds}ms\r\n", dicSync.Name,
                    info.RecordSize, tsObj.TotalMilliseconds);
            }

            if (maxRecordsAllowed > 0 && info.RecordSize > maxRecordsAllowed)
            {
                throw new JJMasterDataException(
                    Translate.Key("Number maximum of records exceeded on {0}, contact the administrator.",
                        dicSync.Name));
            }
        }

        var timeSpan = DateTime.Now - dStart;
        syncInfo.TotalProcessMilliseconds = timeSpan.TotalMilliseconds;

        var logMessage = new StringBuilder();
        logMessage.AppendLine($"UserId: {userId}");
        logMessage.Append(Translate.Key("Synchronizing"));
        logMessage.Append(listSync.Length);
        logMessage.Append(Translate.Key("objects"));
        logMessage.Append(' ');
        logMessage.AppendLine(" ...");
        logMessage.AppendLine(Translate.Key("{0} records analyzed in {1}", totRecords, Format.FormatTimeSpan(timeSpan)));
        
        Logger.LogInformation(logMessage.ToString());
        
        if (syncInfo.ListElement.Count == 0)
            throw new KeyNotFoundException(Translate.Key("No dictionary found"));

        return syncInfo;
    }

    private Hashtable GetSyncInfoFilter(string? userId, Metadata metadata, Hashtable? metadataFilters)
    {
        var filters = new Hashtable();
        var fields = metadata.Table.Fields;
        if (metadataFilters != null)
        {
            foreach (DictionaryEntry osFilter in metadataFilters)
            {
                if (!fields.ContainsKey(osFilter.Key.ToString()))
                    continue;

                filters.Add(fields[osFilter.Key.ToString()].Name, osFilter.Value);
            }
        }

        string fieldApplyUser = metadata.Api.ApplyUserIdOn;

        if (string.IsNullOrEmpty(fieldApplyUser))
            return filters;
        if (!filters.ContainsKey(fieldApplyUser))
        {
            filters.Add(fieldApplyUser, userId);
        }
        else
        {
            if (!filters[fieldApplyUser]!.ToString()!.Equals(userId))
                throw new UnauthorizedAccessException(Translate.Key("Access denied to change user filter on {0}",
                    metadata.Table.Name));
        }

        return filters;
    }
}