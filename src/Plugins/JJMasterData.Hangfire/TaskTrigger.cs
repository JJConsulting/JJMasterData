using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Console;
using Hangfire.Console.Progress;
using Hangfire.Server;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Tasks;
using JJMasterData.Core.DataManager.Exports.Abstractions;
using JJMasterData.Core.DataManager.Imports;

namespace JJMasterData.Hangfire;

internal class TaskTrigger
{
    private BackgroundTask _backgroundTask;
    public BackgroundTask BackgroundTask
    {
        get
        {
            if (_backgroundTask == null)
                _backgroundTask = new BackgroundTask();

            return _backgroundTask;
        }
    }

    public string RunInBackground(string key, IBackgroundTaskWorker worker)
    {
        string jobId = null;
        var token = CancellationToken.None;

        if (worker is IPdfWriter)
        {
            jobId = BackgroundJob.Enqueue(() => ExportToPdf(key, token, null));
        }
        else if (worker is IExcelWriter)
        {
            jobId = BackgroundJob.Enqueue(() => ExportToExcel(key, token, null));
        }
        else if (worker is ITextWriter)
        {
            jobId = BackgroundJob.Enqueue(() => ExportToCsv(key, token, null));
        }
        else if (worker is ImpTextWorker)
        {
            jobId = BackgroundJob.Enqueue(() => ImpFile(key, token, null));
        }
        else
        {
            jobId = BackgroundJob.Enqueue(() => DoProcess(key, token, null));
        }

        return jobId;
    }

    [DisplayName("Exportar para pdf")]
    public Task ExportToPdf(string key, CancellationToken token, PerformContext context)
    {
        return DoProcess(key, token, context);
    }

    [DisplayName("Exportar para excel")]
    public Task ExportToExcel(string key, CancellationToken token, PerformContext context)
    {
        return DoProcess(key, token, context);
    }

    [DisplayName("Exportar para csv")]
    public Task ExportToCsv(string key, CancellationToken token, PerformContext context)
    {
        return DoProcess(key, token, context);
    }

    [DisplayName("Importar Arquivo")]
    public Task ImpFile(string key, CancellationToken token, PerformContext context)
    {
        return DoProcess(key, token, context);
    }

    public Task DoProcess(string key, CancellationToken token, PerformContext context)
    {
        var taskWrapper = BackgroundTask.GetTask(key);
        if (taskWrapper?.TaskWorker == null)
            throw new JJMasterDataException(Translate.Key("This task has expired and can no longer run"));

        IBackgroundTaskWorker worker = taskWrapper.TaskWorker;
        IProgressBar consoleProgress = null;
        string consoleMessage = string.Empty;
            
        worker.OnProgressChanged += (_, e) =>
        {
            BackgroundTask.SetProgress(key, e);
            if (context == null) return;
            if (string.IsNullOrEmpty(e.Message)) return;

            if (consoleProgress == null)
            {
                consoleProgress = context.WriteProgressBar();
                if (!string.IsNullOrEmpty(e.UserId))
                    context.WriteLine(Translate.Key("User [{0}] started the process.", e.UserId));
            }

            if (consoleProgress != null)
            {
                consoleProgress.SetValue(e.Percentage);
            }
                    
            if (!consoleMessage.Equals(e.Message))
            {
                consoleMessage = e.Message;
                context.WriteLine(consoleMessage);
            }

        };

        return worker.RunWorkerAsync(token);
    }


}