#nullable disable warnings
using System;
using JJMasterData.Commons.Tasks.Progress;

namespace JJMasterData.Core.DataManager.Exportation;

public class DataExportationReporter : ProgressReporter
{
    public int TotalProcessed
    {
        get;
        set
        {
            field = value;
            UpdatePercentage();
        }
    }

    public int TotalOfRecords
    {
        get;
        set
        {
            field = value;
            UpdatePercentage();
        }
    }

    public string FolderPath { get; set; }
    public string FileName { get; set; }

    private void UpdatePercentage()
    {
        if (TotalOfRecords > 0 && TotalProcessed > 0)
            Percentage = (int)Math.Round(TotalProcessed / (double)TotalOfRecords * 100);
    }
}
