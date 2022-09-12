using System;

namespace JJMasterData.Commons.Tasks.Progress;

public class ProgressReporter : IProgressReporter
{
    public string UserId { get; set; }

    public int Percentage { get; set; }

    public bool HasError { get; set; }

    public string Message { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }
    
}
