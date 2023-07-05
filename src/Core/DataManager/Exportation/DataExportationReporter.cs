using System;
using JJMasterData.Commons.Tasks.Progress;

namespace JJMasterData.Core.DataManager.Exports;

public class DataExportationReporter : ProgressReporter
{
    private int _totalProcessed;
    public int TotalProcessed
    {
        get => _totalProcessed;
        set
        {
            _totalProcessed = value;
            UpdatePercentage();
        }
    }

    private int _totalRecords;
    public int TotalRecords
    {
        get => _totalRecords;
        set
        {
            _totalRecords = value;
            UpdatePercentage();
        }
    }

    public string FilePath { get; set; }

    private void UpdatePercentage()
    {
        if (TotalRecords > 0 && TotalProcessed > 0)
            Percentage = (int)Math.Round((TotalProcessed / (double)TotalRecords) * 100);
    }
}
