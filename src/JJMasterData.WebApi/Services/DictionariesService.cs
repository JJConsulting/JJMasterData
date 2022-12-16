using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Logging;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository;
using System.Collections;
using System.Text;
using JJMasterData.Commons.Exceptions;
using JJMasterData.WebApi.Models;

namespace JJMasterData.WebApi.Services;

public class DictionariesService
{
    private IEntityRepository _entityRepository;
    private IDataDictionaryRepository _dataDictionaryRepository;

    public DictionariesService(IDataDictionaryRepository dataDictionaryRepository, IEntityRepository entityRepository)
    {
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
        var dictionaries = _dataDictionaryRepository.GetMetadataList(true);
        var syncInfo = new DicSyncInfo
        {
            ServerDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
        };
        int totRecords = 0;
        foreach (var os in listSync)
        {
            var dStartObj = DateTime.Now;
            var dictionary = dictionaries.First(x => x.Table.Name.Equals(os.Name));
            if (dictionary == null)
                throw new JJMasterDataException(Translate.Key("Dictionary {0} not found or not configured for sync", os.Name));

            var filters = GetSyncInfoFilter(userId, dictionary, os.Filters);
            var info = new DicSyncInfoElement();
            info.Name = os.Name;
            info.RecordSize = _entityRepository.GetCount(dictionary.Table, filters);
            totRecords += info.RecordSize;

            TimeSpan tsObj = DateTime.Now - dStartObj;
            info.ProcessMilliseconds = tsObj.TotalMilliseconds;
            syncInfo.ListElement.Add(info);

            if (showLogInfo)
            {
                Log.AddInfo($"- {os.Name}: [{info.RecordSize}] {tsObj.TotalMilliseconds}ms\r\n");
            }

            if (maxRecordsAllowed > 0 && info.RecordSize > maxRecordsAllowed)
            {
                throw new JJMasterDataException(Translate.Key("Number maximum of records exceeded on {0}, contact the administrator.", os.Name));
            }
        }

        TimeSpan ts = DateTime.Now - dStart;
        syncInfo.TotalProcessMilliseconds = ts.TotalMilliseconds;

        var sLog = new StringBuilder();
        sLog.AppendLine($"UserId: {userId}");
        sLog.Append(Translate.Key("Synchronizing"));
        sLog.Append(listSync.Length);
        sLog.Append(Translate.Key("objects"));
        sLog.Append(" ");
        sLog.AppendLine(" ...");
        sLog.AppendLine(Translate.Key("{0} records analyzed in {1}", totRecords, Format.FormatTimeSpan(ts)));
        Log.AddInfo(sLog.ToString());

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
                throw new UnauthorizedAccessException(Translate.Key("Access denied to change user filter on {0}", metadata.Table.Name));
        }

        return filters;
    }

}
