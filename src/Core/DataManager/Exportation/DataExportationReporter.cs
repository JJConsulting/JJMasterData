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

    private int _totalOfRecords;
    public int TotalOfRecords
    {
        get => _totalOfRecords;
        set
        {
            _totalOfRecords = value;
            UpdatePercentage();
        }
    }

    public string FilePath { get; set; }

    private void UpdatePercentage()
    {
        if (TotalOfRecords > 0 && TotalProcessed > 0)
            Percentage = (int)Math.Round((TotalProcessed / (double)TotalOfRecords) * 100);
    }
}
