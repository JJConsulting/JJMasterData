using System;

namespace JJMasterData.Core.DataManager.Exportation;

public sealed class ExportProgressReporter(long? totalRecords, IProgress<ExportProgress> progress)
{
    private const long ReportInterval = 50;
    private long _lastReported;

    public void Report(long processed) => Report(processed, completed: false);

    public void Complete(long processed) => Report(processed, completed: true);

    private void Report(long processed, bool completed)
    {
        if (!completed && processed - _lastReported < ReportInterval)
            return;

        if (processed == _lastReported)
            return;

        _lastReported = processed;
        progress.Report(new ExportProgress(processed, totalRecords, $"Exporting {processed:N0} records..."));
    }
}
