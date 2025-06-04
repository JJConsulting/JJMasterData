using System.Text;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Tasks.Progress;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.DataManager.Importation;

public class DataImportationReporter(IStringLocalizer<MasterDataResources> stringLocalizer) : ProgressReporter
{
    
    private int _totalProcessed;
    public int TotalProcessed 
    {
        get => _totalProcessed;
        set
        {
            _totalProcessed = value;
            UpdatePercentProgress();
        }
    }

    private int _totalRecords;
    public int TotalRecords 
    {
        get => _totalRecords;
        set
        {
            _totalRecords = value;
            UpdatePercentProgress();
        }
    }

    public int Insert { get; set; }
    
    public int Update { get; set; }
    
    public int Delete { get; set; }

    public int Error { get; set; }

    public int Ignore { get; set; }

    public StringBuilder ErrorLog { get; } = new();

    public void AddError(string value)
    {
        ErrorLog.Append("<hr/>");
        ErrorLog.Append("<b>" + stringLocalizer["Row: "] + "</b>");
        ErrorLog.AppendLine(TotalProcessed.ToString());
        ErrorLog.Append(value);
    }

    private void UpdatePercentProgress()
    {
        if (TotalRecords > 0 && TotalProcessed > 0)
            Percentage = (int)(TotalProcessed / (double)TotalRecords * 100);
    }

}

