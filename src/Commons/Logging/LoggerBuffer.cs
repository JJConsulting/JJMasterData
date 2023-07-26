using System.Threading.Channels;

namespace JJMasterData.Commons.Logging;

public abstract class LoggerBuffer
{
    private readonly Channel<char[]> _channel;

    protected LoggerBuffer(int maxSize)
    {
        var options = new BoundedChannelOptions(maxSize)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        };
        _channel = Channel.CreateBounded<char[]>(options);
    }

    public void Enqueue(char[] entry)
    {
        _channel.Writer.TryWrite(entry);
    }

    public bool TryDequeue(out char[] entry)
    {
        return _channel.Reader.TryRead(out entry);
    }
}