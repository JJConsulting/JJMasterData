using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace JJMasterData.Core.DataDictionary.DictionaryDAL;

[Serializable]
[DataContract]
public class DicSyncInfo
{
    /// <summary>
    /// Lista de dicionários com retorno do count
    /// </summary>
    [DataMember(Name = "listElement")]
    public List<DicSyncInfoElement> ListElement { get; set; }

    /// <summary>
    /// Server date.
    /// Format "yyyy-MM-dd HH:mm"
    /// </summary>
    [DataMember(Name = "serverDate")]
    public string ServerDate { get; set; }

    /// <summary>
    /// Total processing time in milliseconds
    /// </summary>
    [DataMember(Name = "totalProcessMilliseconds")]
    public double TotalProcessMilliseconds { get; set; }


    public DicSyncInfo()
    {
        ListElement = new List<DicSyncInfoElement>();
    } 
}