using System.Threading.Channels;

namespace JJMasterData.Commons.Util;

public abstract class BufferBase<T>
{
    private readonly Channel<T> _channel;

    protected BufferBase(int maxSize)
    {
        var options = new BoundedChannelOptions(maxSize)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        };
        _channel = Channel.CreateBounded<T>(options);
    }

    public void Enqueue(T entry)
    {
        _channel.Writer.TryWrite(entry);
    }

    public bool TryDequeue(out T entry)
    {
        return _channel.Reader.TryRead(out entry);
    }
}