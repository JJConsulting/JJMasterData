namespace JJMasterData.Core.DataManager.Exportation;

internal sealed class ExportProgressReporter(ExportContext context)
{
    private const long ReportInterval = 50;
    private long _lastReported;

    public void Report(long processed, bool completed = false)
    {
        if (!completed && processed - _lastReported < ReportInterval)
            return;

        if (processed == _lastReported)
            return;

        _lastReported = processed;
        context.Progress.Report(new ExportProgress(processed, context.TotalRecords, $"Exporting {processed:N0} records..."));
    }
}
