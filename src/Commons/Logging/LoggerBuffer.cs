using System.Collections.Concurrent;

namespace JJMasterData.Commons.Logging;

public abstract class LoggerBuffer
{ 
    private readonly ConcurrentQueue<LogMessage> _queue;

    protected LoggerBuffer()
    {
        _queue = new ConcurrentQueue<LogMessage>();
    }

    public void Enqueue(LogMessage logMessage)
    {
        _queue.Enqueue(logMessage);
    }

    public bool TryDequeue(out LogMessage message)
    {
        return _queue.TryDequeue(out message);
    }
}