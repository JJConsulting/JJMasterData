using System.Text;
using JJMasterData.Commons.Tasks.Progress;

namespace JJMasterData.Core.DataManager.Importation;

public class DataImportationReporter : ProgressReporter
{
    
    private int _totalProcessed;
    public int TotalProcessed 
    {
        get => _totalProcessed;
        set
        {
            _totalProcessed = value;
            UpdatePencertProcess();
        }
    }

    private int _totalRecords;
    public int TotalRecords 
    {
        get => _totalRecords;
        set
        {
            _totalRecords = value;
            UpdatePencertProcess();
        }
    }

    public int Insert { get; set; }
    
    public int Update { get; set; }
    
    public int Delete { get; set; }

    public int Error { get; set; }

    public int Ignore { get; set; }

    public StringBuilder ErrorLog { get; set; }


    public DataImportationReporter()
    {
        ErrorLog = new StringBuilder();
    }

    public void AddError(string value)
    {
        ErrorLog.Append("Row:");
        ErrorLog.Append(" ");
        ErrorLog.AppendLine(TotalProcessed.ToString());
        ErrorLog.Append(value);
        ErrorLog.Append("<hr/>");
    }

    private void UpdatePencertProcess()
    {
        if (TotalRecords > 0 && TotalProcessed > 0)
            Percentage = (int)((TotalProcessed / (double)TotalRecords) * 100);
    }

}

