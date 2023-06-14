using System;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Commons.Tasks.Progress;

namespace JJMasterData.Commons.Tasks;

public interface IBackgroundTaskWorker 
{
    
    public event EventHandler<IProgressReporter> OnProgressChanged;

    public Task RunWorkerAsync(CancellationToken token);
}
