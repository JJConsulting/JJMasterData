using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Console;
using Hangfire.Console.Progress;
using Hangfire.Server;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Tasks;
using JJMasterData.Core.DataManager.Exports.Abstractions;
using JJMasterData.Core.DataManager.Imports;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Hangfire;

internal class TaskTrigger
{
    private BackgroundTaskManager BackgroundTaskManager { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }

    public TaskTrigger(BackgroundTaskManager backgroundTaskManager, IStringLocalizer<JJMasterDataResources> stringLocalizer)
    {
        BackgroundTaskManager = backgroundTaskManager;
        StringLocalizer = stringLocalizer;
    }

    public string RunInBackground(string key, IBackgroundTaskWorker worker)
    {
        string jobId;
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
        else if (worker is DataImportationWorker)
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
        var taskWrapper = BackgroundTaskManager.GetTask(key);
        if (taskWrapper?.TaskWorker == null)
            throw new JJMasterDataException("This task has expired and can no longer run");

        IBackgroundTaskWorker worker = taskWrapper.TaskWorker;
        IProgressBar consoleProgress = null;
        string consoleMessage = string.Empty;
            
        worker.OnProgressChanged += (_, e) =>
        {
            BackgroundTaskManager.SetProgress(key, e);
            if (context == null) return;
            if (string.IsNullOrEmpty(e.Message)) return;

            if (consoleProgress == null)
            {
                consoleProgress = context.WriteProgressBar();
                if (!string.IsNullOrEmpty(e.UserId))
                    context.WriteLine(StringLocalizer["User [{0}] started the process."], e.UserId);
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