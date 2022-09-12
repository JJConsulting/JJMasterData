using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.AuditLog;
using Newtonsoft.Json;

namespace JJMasterData.Core.WebComponents;

[DataContract]
internal class JJDataImpProcess
{
    [XmlIgnore]
    [JsonIgnore]
    public StringBuilder ErrorLog { get; set; }

    [XmlIgnore]
    [JsonIgnore]
    public string ResumeLog { get; set; }

    [XmlIgnore]
    [JsonIgnore]
    public AuditLogData SystemInfo { get; set; }
    
    [XmlIgnore]
    [JsonIgnore]
    public Thread ThreadProcess { get; set; }

    [DataMember(Name = "FileName")]
    public string FileName { get; set; }

    [DataMember(Name = "StartDate")]
    public string StartDate { get; set; }

    [XmlIgnore]
    [JsonIgnore]
    public string EndDate { get; set; }

    [DataMember(Name = "TotalProcessed")]
    public int TotalProcessed { get; set; }

    [DataMember(Name = "TotalRecords")]
    public int TotalRecords { get; set; }

    [DataMember(Name = "Insert")]
    public int Insert { get; set; }

    [DataMember(Name = "Update")]
    public int Update { get; set; }

    [DataMember(Name = "Delete")]
    public int Delete { get; set; }

    [DataMember(Name = "Error")]
    public int Error { get; set; }

    [DataMember(Name = "Ignore")]
    public int Ignore { get; set; }

    [DataMember(Name = "IsProcessing")]
    public bool IsProcessing => ThreadProcess != null && ThreadProcess.IsAlive;

    [DataMember(Name = "PercentProcess")]
    public int PercentProcess
    {
        get
        {
            int percent = 0;
            if (TotalRecords > 0 && TotalProcessed > 0)
                percent = (int)((TotalProcessed / (double)TotalRecords) * 100);

            return percent;
        }
    }

    public void ResetValues()
    {
        ErrorLog = new StringBuilder();
        ResumeLog = string.Empty;
        SystemInfo = null;
        FileName = null;
        TotalRecords = 0;
        TotalProcessed = 0;
        Insert = 0;
        Update = 0;
        Delete = 0;
        Ignore = 0;
        Error = 0;
        StartDate = DateTime.Now.ToDateTimeString();
        EndDate = null;
        
        
    }

    public string GetElapsedTime()
    {
        DateTime startDate;
        DateTime endDate;
        string format = Format.DateTimeFormat;
        string elapsedTime = string.Empty;
        if (DateTime.TryParseExact(StartDate, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out startDate) &&
            DateTime.TryParseExact(EndDate, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate))
        {
            elapsedTime = Format.FormatTimeSpan(startDate, endDate);
        }

        return elapsedTime;
    }

    public void AddError(string value)
    {
        ErrorLog.Append(Translate.Key("Row:"));
        ErrorLog.Append(" ");
        ErrorLog.AppendLine(TotalProcessed.ToString());
        ErrorLog.Append(value);
        ErrorLog.Append("<hr/>");
    }

}
