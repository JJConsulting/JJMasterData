using System.Threading.Channels;

namespace JJMasterData.Commons.Logging;

public abstract class LoggerBuffer
{
    private readonly Channel<LogMessage> _channel;

    protected LoggerBuffer(int maxSize)
    {
        var options = new BoundedChannelOptions(maxSize)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        };
        _channel = Channel.CreateBounded<LogMessage>(options);
    }

    public void Enqueue(LogMessage entry)
    {
        _channel.Writer.TryWrite(entry);
    }

    public bool TryDequeue(out LogMessage entry)
    {
        return _channel.Reader.TryRead(out entry);
    }
}