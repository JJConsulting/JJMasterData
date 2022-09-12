using JJMasterData.Commons.Tasks.Progress;

namespace JJMasterData.Core.DataManager.Exports;

public class DataExpReporter : ProgressReporter
{
    private int _totalProcessed;
    public int TotalProcessed
    {
        get
        {
            return _totalProcessed;
        }
        set
        {
            _totalProcessed = value;
            UpdatePercentage();
        }
    }

    private int _totalRecords;
    public int TotalRecords
    {
        get
        {
            return _totalRecords;
        }
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
            Percentage = (int)((TotalProcessed / (double)TotalRecords) * 100);

    }
}
