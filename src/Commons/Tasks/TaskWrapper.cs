using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Commons.Tasks.Progress;

namespace JJMasterData.Commons.Tasks;

internal sealed class TaskWrapper
{
    public string Key { get; internal set; }

    internal Task Task { get; set; }

    internal CancellationTokenSource CancellationSource { get; set; }

    public IProgressReporter ProgressResult { get; set; }

}
